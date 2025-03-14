/*! @file	VAEDecoder.cs
	@brief	VAEDecoder制御クラス

	@author miura
 */
using UnityEngine;
using Unity.Sentis;

namespace SentisSD
{
	public class VAEDecoder : Model
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private Tensor<float>		m_latentSample;
		/*----------------------------------------------------------------------------------------------------------*/
		public void Set(Tensor<float> latentSample)
		{
			m_latentSample = latentSample;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override string getModelDirectoryName()
		{
			return "vae_decoder";
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override Tensor[] generateInputsTensor()
		{			
			return new Tensor[] { m_latentSample };
		}
	}	// class VAEDecoder
}	// namespace SentisSD
