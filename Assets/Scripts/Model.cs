/*! @file	Model.cs
	@brief	モデル制御基底クラス

	@author miura
 */
using UnityEngine;
using Unity.Sentis;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

namespace SentisSD
{
	public abstract class Model : MonoBehaviour
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private enum Step
		{
			Idle,
			Load,
			Set,
			Inference,
			Wait,
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private string					m_modelPath;
		private Unity.Sentis.Model		m_model;
		private Worker					m_worker;
		private Tensor[]				m_inputsTensor;
		private Tensor<float>			m_outputTensor;
		private float					m_lastTime;
		private Step					m_step;
		/*----------------------------------------------------------------------------------------------------------*/
		public void Start()
		{
			m_modelPath = Path.Join(Application.streamingAssetsPath, "Models/" + getModelDirectoryName() + "/model.sentis");
			m_model = null;
			m_worker = null;
			m_inputsTensor = null;
			m_outputTensor = null;
			m_step = Step.Idle;
			
			start();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void Update()
		{
			switch(m_step)
			{
				case Step.Set:
					cleanup();
					m_worker = new Worker(m_model, BackendType.GPUCompute);
					m_lastTime = Time.realtimeSinceStartup;
					m_step = Step.Inference;
					break;
				case Step.Inference:
					StartCoroutine(inference());
					break;
				default:
					break;
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void OnDestroy()
		{
			cleanup();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void Inference()
		{
			if(m_step != Step.Idle) return;
			load();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public bool IsInferenceCompleted()
		{
			return m_step == Step.Idle;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public Tensor<float> GetOutputTensor()
		{
			return m_outputTensor;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected void replaceOutputTensor(Tensor<float> tensor)
		{
			if(m_outputTensor != null) m_outputTensor.Dispose();
			m_outputTensor = tensor;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected virtual void start() {}
		protected virtual void postInference() {}
		protected virtual bool isInferenceCompleted() { return true; }
		/*----------------------------------------------------------------------------------------------------------*/
		protected abstract string getModelDirectoryName();
		protected abstract Tensor[] generateInputsTensor();
		/*----------------------------------------------------------------------------------------------------------*/
		private async void load()
		{
			if(m_model != null)
			{
				m_step = Step.Set;
				return;
			}
			
			m_lastTime = Time.realtimeSinceStartup;
			
			m_step = Step.Load;
			await Task.Run(()=>
				{
					m_model = ModelLoader.Load(m_modelPath);
				});
			m_step = Step.Set;
			
			Debug.Log("Load Completed " + getModelDirectoryName() + " " + (Time.realtimeSinceStartup - m_lastTime).ToString() + "s");
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private IEnumerator inference()
		{
			m_step = Step.Wait;
			
			m_inputsTensor = generateInputsTensor();
			m_worker.Schedule(m_inputsTensor);
			var tmpTensor = m_worker.PeekOutput() as Tensor<float>;
			var awaiter = tmpTensor.ReadbackAndCloneAsync().GetAwaiter();
			while(!awaiter.IsCompleted)
			{
				yield return null;
			}
			m_outputTensor = awaiter.GetResult();
			
			tmpTensor.Dispose();
			postInference();
			
			if(isInferenceCompleted())
			{
				m_step = Step.Idle;
				Debug.Log(this.GetType().Name + " Inference Completed " + (Time.realtimeSinceStartup - m_lastTime).ToString() + "s");
			}
			else 
			{
				m_outputTensor.Dispose();
				m_step = Step.Inference;
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void cleanup()
		{
			if(m_worker != null) m_worker.Dispose();
			if(m_inputsTensor != null)
			{
				foreach(var inputTensor in m_inputsTensor)
				{
					inputTensor.Dispose();
				}
			}
			if(m_outputTensor != null) m_outputTensor.Dispose();
		}
	}	// class Model
}	// namespace SentisSD
