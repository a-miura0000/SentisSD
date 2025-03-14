/*! @file	UNet.cs
	@brief	UNet制御クラス

	@author miura
 */
using UnityEngine;
using Unity.Sentis;

namespace SentisSD
{
	public class UNet : Model
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private Tensor<float>			m_hiddenStatesTensor;
		private Tensor<float>			m_sampleTensor;
		/*----------------------------------------------------------------------------------------------------------*/
		public void Set(Tensor<float> hiddenStatesTensor, Tensor<float> sampleTensor)
		{
			m_hiddenStatesTensor = hiddenStatesTensor;
			m_sampleTensor = sampleTensor;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override string getModelDirectoryName()
		{
			return "unet";
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override Tensor[] generateInputsTensor()
		{
			return new Tensor[] { m_sampleTensor, new Tensor<int>(new TensorShape(1), new int[] { 50 } ), m_hiddenStatesTensor };
		}
	}	// class UNet
}	// namespace SentisSD
