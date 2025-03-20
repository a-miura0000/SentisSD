# SentisSD
Unity SentisとStableDiffusionを使用して画像生成を行うサンプルです。

## 動作確認済み環境
- **OS**: Windows 11
- **CPU**:Intel Core i7-12700(12コア、2.1GHz)
- **GPU**:NVIDIA GeForce RTX 3060(12GB)
- **Unity**:6000.0.40f1  
- **Unity Sentis**:2.1.2

## 実行手順
1. [Stable Diffusion Models v1.4](https://huggingface.co/CompVis/stable-diffusion-v1-4/tree/onnx)から以下のコマンドで `onnx` ブランチをクローンします。
	```
	git clone https://huggingface.co/CompVis/stable-diffusion-v1-4 -b onnx
	```
	ダウンロードしたフォルダを、`Assets`フォルダ内に配置します。
2. Unityを起動し、ダウンロードした`text_encoder`、`unet`、`vae_decoder`フォルダ内の`model.onnx`を選択します。
   - インスペクタで「**Serialize To StreamingAssets**」ボタンを押します。
3. `./Assets/StreamingAssets` の直下に `model.sentis` が作成されます。
   - その後、`./Assets/StreamingAssets/text_encoder/model.sentis` となるようにフォルダ構成を整えてください。
   - `unet` と `vae_decoder` についても同様に行ってください。
4. ダウンロードした `tokenizer` フォルダを `./Assets/StreamingAssets` にコピーします。
5. `./Assets/Scenes/Main.unity`を開きます。
6. Unityエディタを再生して、`Prompt`フィールドにテキストを入力(英語のみ対応)し、「**Generate**」ボタンを押すと画像が生成されます。
