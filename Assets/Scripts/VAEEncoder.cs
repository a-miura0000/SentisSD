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
		protected override string getModelDirectoryName()
		{
			return "vae_encoder";
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override Tensor[] generateInputsTensor()
		{
			int height = 256;
			int width = 256;
			int channel = 3;
			var inputTensor = new Tensor<float>(new TensorShape(1, channel, height, width));
			for(int c = 0; c < channel; ++c) {
				for(int h = 0; h < height; ++h) {
					for(int w = 0; w < width; ++w) {
						inputTensor[0, c, h, w] = 1f;
					}
				}
			}
			
			return new Tensor[] { inputTensor };
		}
	}	// class VAEEncoder
}	// namespace SentisSD
