/*! @file	VAEEncoder.cs
	@brief	VAEEncoder制御クラス

	@author miura
 */
using UnityEngine;
using Unity.Sentis;

namespace SentisSD
{
	public class VAEEncoder : Model
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private int			m_height;
		private int			m_width;
		/*----------------------------------------------------------------------------------------------------------*/
		public void Set(int height, int width)
		{
			m_height = height;
			m_width = width;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override string getModelDirectoryName()
		{
			return "vae_encoder";
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override Tensor[] generateInputsTensor()
		{
			int channel = 3;
			var inputTensor = new Tensor<float>(new TensorShape(1, channel, m_height, m_width));
			for(int c = 0; c < channel; ++c) {
				for(int h = 0; h < m_height; ++h) {
					for(int w = 0; w < m_width; ++w) {
						inputTensor[0, c, h, w] = 1f;
					}
				}
			}
			
			return new Tensor[] { inputTensor };
		}
	}	// class VAEEncoder
}	// namespace SentisSD
