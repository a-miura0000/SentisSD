/*! @file	TextEncoder.cs
	@brief	TextEncoder制御クラス

	@author miura
 */
using UnityEngine;
using Unity.Sentis;

namespace SentisSD
{
	public class TextEncoder : Model
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public void Set(string prompt)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override string getModelDirectoryName()
		{
			return "text_encoder";
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override Tensor[] generateInputsTensor()
		{
		//	int[] inputIds = {49406, 49407, 49407, 49407, 49407, 49407, 49407, 49407, 49407, 49407, 49407,
		//						49406, 320, 23050, 530, 550, 896, 14178, 530, 518, 6267, 49407};
			int[] inputIds = {49406, 49407, 49407,
								49406, 2368, 49407};
			return new Tensor[] { new Tensor<int>(new TensorShape(2, 3), inputIds) };
		}
	}	// class TextEncoder
}	// namespace SentisSD
