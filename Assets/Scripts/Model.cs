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
			Wait,
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private string					m_modelPath;
		private Unity.Sentis.Model		m_model;
		private Worker					m_worker;
		private Tensor<float>			m_outputTensor;
		private Step					m_step;
		/*----------------------------------------------------------------------------------------------------------*/
		public void Start()
		{
			m_modelPath = Path.Join(Application.streamingAssetsPath, "Models/" + getModelDirectoryName() + "/model.sentis");
			m_model = null;
			m_worker = null;
			m_outputTensor = null;
			m_step = Step.Idle;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void Update()
		{
			switch(m_step)
			{
				case Step.Set:
					StartCoroutine(inference());
					break;
				default:
					break;
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void OnDestroy()
		{
			if(m_worker != null) m_worker.Dispose();
			if(m_outputTensor != null) m_outputTensor.Dispose();
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
			
			m_step = Step.Load;
			await Task.Run(()=>
				{
					m_model = ModelLoader.Load(m_modelPath);
				});
			m_worker = new Worker(m_model, BackendType.GPUCompute);
			m_step = Step.Set;
			
			Debug.Log("LoadCompleted " + getModelDirectoryName());
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private IEnumerator inference()
		{
			m_step = Step.Wait;
			
			var inputsTensor = generateInputsTensor();
			m_worker.Schedule(inputsTensor);
			var tmpTensor = m_worker.PeekOutput() as Tensor<float>;
			var awaiter = tmpTensor.ReadbackAndCloneAsync().GetAwaiter();
			while(!awaiter.IsCompleted)
			{
				yield return null;
			}
			m_outputTensor = awaiter.GetResult();
			
			tmpTensor.Dispose();	
			foreach(var inputTensor in inputsTensor)
			{
				inputTensor.Dispose();
			}
			
			m_step = Step.Idle;
			
			Debug.Log(this.GetType().Name + " InferenceCompleted");
		}
	}	// class Model
}	// namespace SentisSD
