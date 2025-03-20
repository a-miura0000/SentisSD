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
		private Tensor<int>			m_inputIdsTensor;
		/*----------------------------------------------------------------------------------------------------------*/
		public void Set(Tensor<int> inputIdsTensor)
		{
			m_inputIdsTensor = inputIdsTensor;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override string getModelDirectoryName()
		{
			return "text_encoder";
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override Tensor[] generateInputsTensor()
		{
			return new Tensor[] { m_inputIdsTensor };
		}
	}	// class TextEncoder
}	// namespace SentisSD
