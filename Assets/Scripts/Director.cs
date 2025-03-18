/*! @file	Director.cs
	@brief	制御用クラス

	@author miura
 */
using UnityEngine;

namespace SentisSD
{
	public class Director : MonoBehaviour
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private enum Status
		{
			Idle,
			TextEncode,
			UNet,
			VAEDecode,
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private UI						m_ui;
		private TextEncoder				m_textEncoder;
		private UNet					m_unet;
		private VAEDecoder				m_vaeDecoder;
		private bool					m_isGenerate;
		private Status					m_status;
		/*----------------------------------------------------------------------------------------------------------*/
		public void Start()
		{
			var uiObject = new GameObject("UI");
			uiObject.transform.SetParent(transform);
			m_ui = uiObject.AddComponent<UI>();
			m_ui.AddListener(() => { m_isGenerate = true; });
			
			
			var modelParent = new GameObject("Models").transform;
			modelParent.SetParent(transform);
			
			m_textEncoder = createModel<TextEncoder>(modelParent);
			m_unet = createModel<UNet>(modelParent);
			m_vaeDecoder = createModel<VAEDecoder>(modelParent);
			
			m_isGenerate = false;
			m_status = Status.Idle;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void Update()
		{
			switch(m_status)
			{
				case Status.Idle:
					if(!m_isGenerate) break;
					m_isGenerate = false;
					m_ui.InferenceStart();
					m_textEncoder.Set(m_ui.Prompt);
					m_textEncoder.Inference();
					m_status = Status.TextEncode;
					break;
				case Status.TextEncode:
					if(!m_textEncoder.IsInferenceCompleted()) break;
					m_unet.Set(m_textEncoder.GetOutputTensor(), 512, 512, 7.5f, 15);
					m_unet.Inference();
					m_status = Status.UNet;
					break;
				case Status.UNet:
					if(!m_unet.IsInferenceCompleted()) break;
					m_vaeDecoder.Set(m_unet.GetOutputTensor());
					m_vaeDecoder.Inference();
					m_status = Status.VAEDecode;
					break;
				case Status.VAEDecode:
					if(!m_vaeDecoder.IsInferenceCompleted()) break;
					m_ui.GenerateImage(m_vaeDecoder.GetOutputTensor(), 512, 512);
					m_ui.InferenceEnd();
					m_status = Status.Idle;
					break;
				default:
					break;
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private T createModel<T>(Transform parent) where T : Model
		{
			var type = typeof(T);
			var gameObject = new GameObject(type.Name, type);
			gameObject.transform.SetParent(parent);
			
			return gameObject.GetComponent<T>();
		}
	}	// class Director
}	// namespace SentisSD
