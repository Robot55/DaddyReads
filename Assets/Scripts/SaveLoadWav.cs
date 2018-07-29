﻿/* Copyright (c) 2018 Manuel T. Schrempf

This software is provided 'as-is', without any express or implied warranty. In
no event will the authors be held liable for any damages arising from the use
of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it freely,
subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim
that you wrote the original software. If you use this software in a product,
an acknowledgment in the product documentation would be appreciated but is not
required.

2. Altered source versions must be plainly marked as such, and must not be
misrepresented as being the original software.

3. This notice may not be removed or altered from any source distribution.

=============================================================================

derived from darktables's script
https://gist.github.com/darktable/2317063 
with corrected errors and an additional load method.*/

using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

// Saves and loads in Unity .wav files in the Application.persistentDataPath
public class SaveLoadWav {
	const int HEADER_SIZE = 44;

	// Saves the audio clip as .wav file in the Application.persistentDataPath
	public void Save(string filename, AudioClip clip, bool makeClipShort = true)
	{
		if(!String.IsNullOrEmpty(filename) && clip != null){
			string filepath = GetPath(filename);
			Directory.CreateDirectory(Path.GetDirectoryName(filepath)); // Make sure directory exists if user is saving to sub dir.

			if (makeClipShort)
				clip = TrimSilence(clip, 0);

			using (var fileStream = CreateEmpty(filepath))
			{
				ConvertAndWrite(fileStream, clip);
				WriteHeader(fileStream, clip);
			}
		}
	}

	// Assigns the loaded audio clip to the source or does nothing if the argument filename or path is inexistend. Call this method inside the StartCoroutine of C#
	public IEnumerator<WWW> Load(string filename, AudioSource audioSource){
		if(!String.IsNullOrEmpty(filename) && audioSource != null){
			string path = GetPath(filename);

			if(File.Exists(path))
			{
				WWW www = new WWW("file:" + path);
				yield return www;
				audioSource.clip = www.GetAudioClip(false, false, AudioType.WAV);
				audioSource.clip.name = filename;
			}
		}
		yield break;
	}

	// Returns the used path with filename and its ending
	private string GetPath(string filename){
		return Path.Combine(Application.persistentDataPath, filename.EndsWith(".wav") ? filename : filename + ".wav");
	}

	// Reduces silence in the .wav
	public AudioClip TrimSilence(AudioClip clip, float min) {
		float[] samples = new float[clip.samples];
		clip.GetData(samples, 0);
		return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
	}

	// Reduces silence in the .wav
	public AudioClip TrimSilence(List<float> samples, float min, int channels, int hz) {
		return TrimSilence(samples, min, channels, hz, false);
	}

	// Reduces silence in the .wav
	public AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool stream) {
		int i;

		for (i=0; i<samples.Count; i++)
			if (Mathf.Abs(samples[i]) > min) {
				break;
			}

		samples.RemoveRange(0, i);

		for (i=samples.Count - 1; i>0; i--)
			if (Mathf.Abs(samples[i]) > min) {
				break;
			}

		samples.RemoveRange(i, samples.Count - i);

		AudioClip clip = AudioClip.Create("TempClip", samples.Count, channels, hz, stream);

		clip.SetData(samples.ToArray(), 0);

		return clip;
	}

	// Creates an empty file stream and prepairs the header
	private FileStream CreateEmpty(string filepath) {
		FileStream fileStream = new FileStream(filepath, FileMode.Create);
		byte emptyByte = new byte();

		for(int i = 0; i < HEADER_SIZE; i++) //preparing the header
			fileStream.WriteByte(emptyByte);

		return fileStream;
	}

	// Converts the clip into the filestream
	private void ConvertAndWrite(FileStream fileStream, AudioClip clip) {
		float[] samples = new float[clip.samples];
		clip.GetData(samples, 0);
		Int16[] intData = new Int16[samples.Length];
		//converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]
		Byte[] bytesData = new Byte[samples.Length * 2];
		//bytesData array is twice the size of
		//dataSource array because a float converted in Int16 is 2 bytes.

		int rescaleFactor = 32767; //to convert float to Int16

		for (int i = 0; i<samples.Length; i++) {
			intData[i] = (short) (samples[i] * rescaleFactor);
			Byte[] byteArr = new Byte[2];
			byteArr = BitConverter.GetBytes(intData[i]);
			byteArr.CopyTo(bytesData, i * 2);
		}

		fileStream.Write(bytesData, 0, bytesData.Length);
	}

	// Writes the .wav header
	private void WriteHeader(FileStream fileStream, AudioClip clip) {
		int hz = clip.frequency;
		int channels = clip.channels;
		int samples = clip.samples;

		fileStream.Seek(0, SeekOrigin.Begin);

		Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(riff, 0, 4);

		Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
		fileStream.Write(chunkSize, 0, 4);

		Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(wave, 0, 4);

		Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(fmt, 0, 4);

		Byte[] subChunk1 = BitConverter.GetBytes(16);
		fileStream.Write(subChunk1, 0, 4);

		UInt16 one = 1;

		Byte[] audioFormat = BitConverter.GetBytes(one);
		fileStream.Write(audioFormat, 0, 2);

		Byte[] numChannels = BitConverter.GetBytes(channels);
		fileStream.Write(numChannels, 0, 2);

		Byte[] sampleRate = BitConverter.GetBytes(hz);
		fileStream.Write(sampleRate, 0, 4);

		Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
		fileStream.Write(byteRate, 0, 4);

		UInt16 blockAlign = (ushort) (channels * 2);
		fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

		UInt16 bps = 16;
		Byte[] bitsPerSample = BitConverter.GetBytes(bps);
		fileStream.Write(bitsPerSample, 0, 2);

		Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
		fileStream.Write(datastring, 0, 4);

		Byte[] subChunk2 = BitConverter.GetBytes(samples * 2);
		fileStream.Write(subChunk2, 0, 4);

		fileStream.Close();
	}
}