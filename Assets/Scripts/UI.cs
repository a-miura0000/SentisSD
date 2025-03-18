/*! @file	UI.cs
	@brief	UI制御用クラス

	@author miura
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Unity.Sentis;
using System.Collections;

namespace SentisSD
{
	public class UI : MonoBehaviour
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private Image			m_image;
		private InputField		m_input;
		private Button			m_button;
		private Text			m_inferenceText;
		private Coroutine		m_coroutine;
		/*----------------------------------------------------------------------------------------------------------*/
		public string			Prompt { get => m_input.text; }
		/*----------------------------------------------------------------------------------------------------------*/
		public void Awake()
		{
			var canvas = gameObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			var canvasScaler = gameObject.AddComponent<CanvasScaler>();
			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvasScaler.referenceResolution = new Vector2(1920, 1080);
			canvasScaler.matchWidthOrHeight = 0f;
			
			gameObject.AddComponent<GraphicRaycaster>();
			gameObject.AddComponent<EventSystem>();
			gameObject.AddComponent<StandaloneInputModule>();

			createImage();
			crateInputField();
			createButton();
			createInferenceText();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void AddListener(UnityAction listener)
		{
			m_button.onClick.AddListener(listener);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void InferenceStart()
		{
			m_coroutine = StartCoroutine(inference());
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void InferenceEnd()
		{
			StopCoroutine(m_coroutine);
			m_coroutine = null;
			m_inferenceText.text = "";
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void GenerateImage(Tensor<float> tensor, int height, int width)
		{
			Texture2D texture = new Texture2D(width, height);
			Color[] pixels = new Color[height * width];
			int index = 0;
			for(int h = height - 1; h >= 0; --h) {
				for(int w = 0; w < width; ++w) {
					pixels[index] = new Color();
					pixels[index].r = Mathf.Clamp(tensor[0, 0, h, w] / 2f + 0.5f, 0f, 1f);
					pixels[index].g = Mathf.Clamp(tensor[0, 1, h, w] / 2f + 0.5f, 0f, 1f);
					pixels[index].b = Mathf.Clamp(tensor[0, 2, h, w] / 2f + 0.5f, 0f, 1f);
					pixels[index].a = 1f;
					index++;
				}
			}
			texture.SetPixels(pixels);
			texture.Apply();
			
			((RectTransform)m_image.transform).sizeDelta = new Vector2(width, height);
			m_image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void createImage()
		{
			var imageObject = new GameObject("Image");
			imageObject.transform.SetParent(transform);
			m_image = imageObject.AddComponent<Image>();
			m_image.color = Color.white;
			m_image.raycastTarget = false;
			var rectTransform = imageObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = Vector2.zero;
			rectTransform.anchoredPosition = Vector2.zero; 
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void crateInputField()
		{
			var inputObject = new GameObject("InputField");
			inputObject.transform.SetParent(transform, false);
			m_input = inputObject.AddComponent<InputField>();
			m_input.lineType = InputField.LineType.MultiLineNewline;
			
			inputObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
			
			var placeholderObject = new GameObject("Placeholder");
			placeholderObject.transform.SetParent(inputObject.transform, false);
			var placeholderText = placeholderObject.AddComponent<Text>();
			setTextParameter(ref placeholderText);
			placeholderText.text = "Prompt";
			placeholderText.color = Color.gray;
			m_input.placeholder = placeholderText;
			
			var textObject = new GameObject("Text");
			textObject.transform.SetParent(inputObject.transform, false);
			var text = textObject.AddComponent<Text>();
			setTextParameter(ref text);
			m_input.textComponent = text;
			
			var rectTransform = inputObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(300f, 300f);
			rectTransform.anchoredPosition = new Vector2(400f, 0f);
			rectTransform.pivot = new Vector2(0, 1);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void createButton()
		{
			var size = new Vector2(150f, 80f);
			
			var buttonObject = new GameObject("Button");
			buttonObject.transform.SetParent(transform);
			var image = buttonObject.AddComponent<Image>();
			image.color = new Color(0.5f, 0.5f, 0.5f, 1f);
			m_button = buttonObject.AddComponent<Button>();
			var rectTransform = buttonObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = size;
			rectTransform.pivot = new Vector2(0, 1);
			rectTransform.anchoredPosition = new Vector2(400f, -300f);
			
			var textObject = new GameObject("Text");
			textObject.transform.SetParent(buttonObject.transform);
			var text = textObject.AddComponent<Text>();
			setTextParameter(ref text);
			text.text = "Generate";
			rectTransform = textObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = size;
			rectTransform.anchoredPosition = Vector2.zero; 
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void createInferenceText()
		{
			var textObject = new GameObject("inference...");
			textObject.transform.SetParent(transform, false);
			m_inferenceText = textObject.AddComponent<Text>();
			setTextParameter(ref m_inferenceText);
			m_inferenceText.fontSize = 80;
			m_inferenceText.alignment = TextAnchor.MiddleLeft;
			m_inferenceText.color = Color.yellow;
			
			var rectTransform = textObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(900f, 200f);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void setTextParameter(ref Text text)
		{
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			text.alignment = TextAnchor.MiddleCenter;
			text.color = Color.black;
			text.fontSize = 30;
			text.raycastTarget = false;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private IEnumerator inference()
		{
			var baseText = "Inference in progress";
			var addText = "";
			int count = 0;
			while(true)
			{
				m_inferenceText.text = baseText + addText;
				if(count++ > 2)
				{
					addText = "";
					count = 0;
				}
				else 
				{
					addText += ".";
				}
				yield return new WaitForSeconds(1f);
			}
		}
	}	// class UI
}	// namespace SentisSD
