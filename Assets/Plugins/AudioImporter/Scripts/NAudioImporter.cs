using System;
using System.IO;
using NAudio.Wave;
using UnityEngine;

/// <summary>
/// A component for importing audio files using NAudio.
/// </summary>
[AddComponentMenu("AudioImporter/NAudio Importer")]
public class NAudioImporter : DecoderImporter
{
    private Mp3FileReader reader;
    private ISampleProvider sampleProvider;
    
    protected override void Initialize()
    {
        MemoryStream stream = new MemoryStream(uri);
        reader = new Mp3FileReader(stream);
        
        sampleProvider = reader.ToSampleProvider();
    }

    protected override void Cleanup()
    {
        if (reader != null)
            reader.Dispose();

        reader = null;
        sampleProvider = null;
    }

    protected override AudioInfo GetInfo()
    {
        WaveFormat format = reader.WaveFormat;
        int lengthSamples = (int)reader.Length / (format.BitsPerSample / 8);
        return new AudioInfo(lengthSamples, format.SampleRate, format.Channels);
    }

    protected override int GetSamples(float[] buffer, int offset, int count)
    {
        return sampleProvider.Read(buffer, offset, count);
    }
}
