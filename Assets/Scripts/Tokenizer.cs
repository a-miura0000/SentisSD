/*! @file	Tokenizer.cs
	@brief	Tokenizerクラス

	@author miura
 */
using UnityEngine;
using Unity.Sentis;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SentisSD
{
	public class Tokenizer : MonoBehaviour
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private enum Step 
		{
			Idle,
			Load,
			Tokenize,
			Finished,
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private string						m_prompt;
		private Dictionary<string, int>		m_dic;
		private Tensor<int>					m_outputTensor;
		private Step						m_step;
		private const int					cm_maxLength = 77;
		private const string				cm_bosToken = "<|startoftext|>";
		private const string				cm_eosToken = "<|endoftext|>";
		private const string				cm_padToken = "<|endoftext|>";
		private const string				cm_unkToken = "<|endoftext|>";
		private const string				cm_wordEndToken = "</w>";
		private static readonly string		cm_dirName = Application.streamingAssetsPath + "/Models/tokenizer/";
		private static readonly string		cm_vocabulary = cm_dirName + "vocab.json";
		/*----------------------------------------------------------------------------------------------------------*/
		public void Start()
		{
			m_prompt = null;
			m_dic = new Dictionary<string, int>();
			m_outputTensor = null;
			load();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void Update()
		{
			switch(m_step)
			{
				case Step.Idle:
					if(m_prompt != null)
					{
						m_step = Step.Tokenize;
					}
					break;
				case Step.Tokenize:
					tokenize();
					m_step = Step.Finished;
					break;
				default:
					break;
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void OnDestroy()
		{
			if(m_outputTensor != null) m_outputTensor.Dispose();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void Encoder(string prompt)
		{
			m_prompt = prompt;
			if(m_outputTensor != null) m_outputTensor.Dispose();
			if(m_step != Step.Load) m_step = Step.Tokenize;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public bool IsCompleted()
		{
			return m_step == Step.Finished;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public Tensor<int> GetOutputTensor()
		{
			return m_outputTensor;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private async void load()
		{			
			m_step = Step.Load;
			
			await Task.Run(()=>
				{
					var json = File.ReadAllText(cm_vocabulary);
					m_dic = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
				});
			m_step = Step.Idle;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void tokenize()
		{
			var words = m_prompt.ToLower().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			int length = Math.Min(words.Length + 2, cm_maxLength);
			int[] ids = new int[length * 2];
			for(int i = 0; i < length; ++i)
			{
				if(i == 0) ids[i] = m_dic[cm_bosToken];
				else if(i == (length - 1)) ids[i] = m_dic[cm_eosToken];
				else ids[i] = m_dic[cm_padToken];
			}
			
			int index = length;
			ids[index++] = m_dic[cm_bosToken];
			foreach(var word in words)
			{
				if(m_dic.ContainsKey(word + cm_wordEndToken)) ids[index++] = m_dic[word + cm_wordEndToken];
				else if(m_dic.ContainsKey(word)) ids[index++] = m_dic[word];
				else ids[index++] = m_dic[cm_unkToken];
			}
			ids[index++] = m_dic[cm_eosToken];
			
			m_outputTensor = new Tensor<int>(new TensorShape(2, length), ids);
		}
	}	// class Tokenizer
}	// namespace SentisSD
