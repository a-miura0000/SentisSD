/*! @file	UI.cs
	@brief	UI制御用クラス

	@author miura
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Unity.Sentis;

namespace SentisSD
{
	public class UI : MonoBehaviour
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private Button			m_button;
		private Image			m_image;
		/*----------------------------------------------------------------------------------------------------------*/
		public void Awake()
		{
			var canvas = gameObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			gameObject.AddComponent<CanvasScaler>();
			gameObject.AddComponent<GraphicRaycaster>();
			gameObject.AddComponent<EventSystem>();
			gameObject.AddComponent<StandaloneInputModule>();

			createButton();
			createImage();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void AddListener(UnityAction listener)
		{
			m_button.onClick.AddListener(listener);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void GenerateImage(Tensor<float> tensor, int height, int width)
		{
			Texture2D texture = new Texture2D(width, height);
			Color[] pixels = new Color[height * width];
			int index = 0;
			for(int h = 0; h < height; ++h) {
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
			rectTransform.anchoredPosition = Vector2.zero; 
			rectTransform.localPosition = new Vector2(0f, 100f);
			
			var textObject = new GameObject("Text");
			textObject.transform.SetParent(buttonObject.transform);
			var text = textObject.AddComponent<Text>();
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			text.text = "Generate";
			text.alignment = TextAnchor.MiddleCenter;
			text.color = Color.black;
			text.fontSize = 28;
			text.raycastTarget = false;
			rectTransform = textObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = size;
			rectTransform.anchoredPosition = Vector2.zero; 
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
	}	// class UI
}	// namespace SentisSD
