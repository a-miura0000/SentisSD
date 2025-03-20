/*! @file	UNet.cs
	@brief	UNet制御クラス

	@author miura
 */
using UnityEngine;
using Unity.Sentis;
using System;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics;

namespace SentisSD
{
	public class UNet : Model
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private struct Data
		{
			public int					timeStep;
			public float				sigma;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private Tensor<float>			m_hiddenStatesTensor;
		private float					m_guidanceScale;
		private int						m_step;
		private TensorShape				m_sampleShape;
		private float[]					m_sampleBuffers;
		private int						m_currentStep;
		private List<float>				m_sigmaList;
		private List<float[]>			m_derivativesList;
		private Data[]					m_data;
		private Tensor[]				m_tmpTensors;
		private const int				cm_traningTimeSteps = 1000;
		private const float				cm_linearStart = 0.00085f;
		private const float				cm_linearEnd = 0.0120f;
		private const float				cm_scaleFactor = 1f / 0.18215f;
		/*----------------------------------------------------------------------------------------------------------*/
		public void Set(Tensor<float> hiddenStatesTensor, int height, int width, float guidanceScale, int step)
		{
			m_hiddenStatesTensor = hiddenStatesTensor;
			m_guidanceScale = guidanceScale;
			m_step = step;
			
			createSample(height, width);
			m_currentStep = 0;
			
			m_data = new Data[m_step];
			
			var indexs = new double[m_step];
			if(m_step > 1)
			{
				double rate = ((double)(cm_traningTimeSteps - 1)) / (m_step - 1);
				for(int i = 0; i < m_step; ++i) indexs[i] = i * rate;
			}
			
			var sigmas = new float[m_step];
			for(int i = 0; i < m_step; ++i)
			{
				int index = (int)indexs[i];
				if((i == 0) || (i == (m_step - 1)))
				{
					m_data[i].sigma = m_sigmaList[index];
				}
				else 
				{
					float t = (float)(indexs[i] - index);
					m_data[i].sigma = (m_sigmaList[index + 1] - m_sigmaList[index]) * t + m_sigmaList[index];
				}
				
				m_data[m_step - 1 - i].timeStep = index;
			}
			
			m_derivativesList.Clear();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override void start()
		{
			// ノイズ
			var rate = (cm_linearEnd - cm_linearStart) / ((cm_traningTimeSteps > 1) ? cm_traningTimeSteps - 1 : 1);
			List<float> betaList = Enumerable.Range(0, cm_traningTimeSteps).Select((i) => cm_linearStart + i * rate).ToList();
			var alphaList = betaList.Select((beta) => 1 - beta).ToList();
			// 累積積
			var cumulativeProductList = alphaList.Select((alpha, i) => alphaList.Take(i + 1).Aggregate((a, b) => a * b)).ToList();
			// シグマ
			m_sigmaList = cumulativeProductList.Select((prod) => (float)(Math.Sqrt((1 - prod) / prod))).Reverse().ToList();
			
			m_derivativesList = new List<float[]>();
			m_tmpTensors = new Tensor[2];
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override void postInference()
		{
			var tensor = GetOutputTensor();
			var modelOutputBuffers = tensor.AsReadOnlyNativeArray().ToArray();

			int count = tensor.count / 2;
			for(int i = 0; i < count; ++i)
			{
				modelOutputBuffers[i] += (modelOutputBuffers[count + i] - modelOutputBuffers[i]) * m_guidanceScale;
			}
			
			linearMultistepMethod(modelOutputBuffers);
			
			m_currentStep++;
			
			foreach(var tmp in m_tmpTensors)
			{
				tmp.Dispose();
			}
			
			if(!isInferenceCompleted()) return;
			
			for(int i = 0; i < m_sampleBuffers.Length; ++i)
			{
				m_sampleBuffers[i] *= cm_scaleFactor;
			}

			var shape = new TensorShape(m_sampleShape[0] / 2, m_sampleShape[1], m_sampleShape[2], m_sampleShape[3]);
			replaceOutputTensor(new Tensor<float>(shape, m_sampleBuffers));
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override bool isInferenceCompleted()
		{
			return m_currentStep >= m_step;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override string getModelDirectoryName()
		{
			return "unet";
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override Tensor[] generateInputsTensor()
		{
			var srcBuffers = m_sampleBuffers;
			int length = srcBuffers.Length;
			float[] destBuffers = new float[length * 2];
			var rate = 1f / (float)Math.Sqrt((Math.Pow(m_data[m_currentStep].sigma, 2) + 1));
			for(int i = 0; i < length; ++i)
			{
				destBuffers[i] = srcBuffers[i] * rate;
				destBuffers[length + i] = destBuffers[i];
			}
			
			m_tmpTensors[0] = new Tensor<float>(m_sampleShape, destBuffers);
			m_tmpTensors[1] = new Tensor<int>(new TensorShape(1), new int[] { m_data[m_currentStep].timeStep } );
			
			return new Tensor[] 
				{ 
					m_tmpTensors[0],
					m_tmpTensors[1],
					m_hiddenStatesTensor
				};
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void createSample(int height, int width)
		{
			int batch = 1;
			int channel = 4;
			height /= 8;
			width /= 8;
			m_sampleShape = new TensorShape(batch * 2, channel, height, width);
			int length = batch * channel * height * width;
			m_sampleBuffers = new float[length];
			
			var random = new System.Random();
			for(int i = 0; i < length; ++i)
			{
				var u0 = random.NextDouble();
				var u1 = random.NextDouble();
				var radius = Math.Sqrt(-2.0 * Math.Log(u0));
				var theta = 2.0 * Math.PI * u1;
				var standardNormalRand = radius * Math.Cos(theta);
				
				m_sampleBuffers[i] = (float)standardNormalRand * m_sigmaList[0];
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void linearMultistepMethod(float[] modelOutputBuffers)
		{
			var length = m_sampleBuffers.Length;
			var derivativeBuffers = new float[length];
			var sigma = m_data[m_currentStep].sigma;
			for(int i = 0; i < m_sampleBuffers.Length; ++i)
			{
				var predSample = m_sampleBuffers[i] - sigma * modelOutputBuffers[i];
				derivativeBuffers[i] = (m_sampleBuffers[i] - predSample) / sigma;
			}
			
			const int maxDerivative = 4;
			m_derivativesList.Add(derivativeBuffers);
			if(m_derivativesList.Count > maxDerivative)
			{
				m_derivativesList.RemoveAt(0);
			}
			
			int num = m_derivativesList.Count;
			var nextSigma = (m_currentStep < (m_data.Length - 1)) ? m_data[m_currentStep + 1].sigma : 0f;
			var sumBuffers = new float[m_sampleBuffers.Length];
			for(int i = 0; i < num; ++i)
			{
				var coefficient = (float)Integrate.OnClosedInterval(
					(double val) =>
					{
						double product = 1.0;
						for(int t = 0; t < num; ++t)
						{
							if(t == i) continue;
							product *= (val - m_data[m_currentStep - t].sigma) / 
								(m_data[m_currentStep - i].sigma - m_data[m_currentStep - t].sigma);
						}
						return product;
					}, sigma, nextSigma, 1e-4);
					
				derivativeBuffers = m_derivativesList[(num - 1) - i];
				for(int t = 0; t < length; ++t)
				{
					sumBuffers[t] += derivativeBuffers[t] * coefficient;
				}
			}
			
			for(int i = 0; i < length; ++i)
			{
				m_sampleBuffers[i] += sumBuffers[i];
			}
		}
	}	// class UNet
}	// namespace SentisSD
