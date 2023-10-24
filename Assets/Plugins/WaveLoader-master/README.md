# WaveLoader

**A quick and dirty WAV file loader for .NET and Unity**

## Overview

This is a dirt-simple, permissively licensed little chunk o' code that can be used to load a WAV file, parse its headers, and turn it into an AudioClip in Unity.

## Usage

If you just want to turn a WAV file into an audio clip in Unity, that's easy:

`AudioClip myAudioClip = WaveLoader.LoadWaveToAudioClip(myByteArray);`

You can also pass in a path to your WAV file, and/or give the audio clip a name:

`AudioClip myAudioClip = WaveLoader.LoadWaveToAudioClip("Path/To/My/Sound.wav", "nameOfAudioClip");`

Or, if you have no interest in AudioClips, you can create a WaveFile object:

`WaveFile myWaveFile = WaveFile.Load(myByteArray);`

If you change your mind about not wanting an AudioClip:

`AudioClip myAudioClip = myWaveFile.ToAudioClip();`

You can't do much else other than look at it yet, though.

## Notes

The Unity-specific stuff is in WaveLoader.cs. Everything in WaveFile.cs doesn't depend on Unity. Yes, ToAudioClip is actually an extension method!

The code does a reasonable amount of error checking and should reject most things that don't look like valid wave files with a FormatException. Some types of WAV file are valid, but not supported for conversion, so they will load but it will then throw NotSupportedException if you ask it to convert the data.

Unlike some of the other code snippets and libraries floating around out there, WaveLoader actually reads the entire RIFF header, parsing chunks it recognizes and ignoring ones it doesn't. That means it can handle non-standard and extra chunks in the file, although it does still require a well-formed `fmt` chunk.

I'm pretty sure this handles 8- and 24-bit WAV files correctly; they sound correct to my ears. But there might be some off-by-one lurking here or there. The 16- and 32-bit (PCM and float) formats are easier to deal with and I'm reasonably confident those are handled correctly. 

There have been attempts at optimization, but I wouldn't call it well-optimized. WaveFile makes a copy of the underlying byte array by default, which is safer if you want to keep the WaveFile object around, but wastes a significant amount of memory if you don't. If you use the WaveLoader.LoadWaveToAudioClip methods, those will override the default and store the same byte array, which is just fine because the WaveFile object doesn't live very long in that case anyway. It also does some magic with delegates in the conversion routine, which might actually be slower than conditionals but to be honest I haven't profiled it.

## Version History

1.0.0 - Initial release.

1.1.0 - Improved header parsing, optimization, and convenience methods.

1.1.1 - Fixed 24-bit signed WAV conversion.

1.1.2 - Fixed handling of chunks with padding byte.

## Acknowledgements

Inspired by [Wav Utility for Unity](https://github.com/deadlyfingers/UnityWav), though mine is an all-new implementation.

I worked off the following sources to figure out how WAV files should be parsed:
- <http://soundfile.sapp.org/doc/WaveFormat/>
- <http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/WAVE.html>
- <https://blogs.msdn.microsoft.com/dawate/2009/06/23/intro-to-audio-programming-part-2-demystifying-the-wav-format/>
- <https://trac.ffmpeg.org/wiki/audio%20types>
- <https://www.recordingblogs.com/wiki/list-chunk-of-a-wave-file>
- <https://www.recordingblogs.com/wiki/fact-chunk-of-a-wave-file>
- <https://en.wikipedia.org/wiki/Resource_Interchange_File_Format>
- <https://en.wikipedia.org/wiki/WAV>
- <http://wavefilegem.com/how_wave_files_work.html>
- <https://docs.rs/riff-wave/0.1.2/riff_wave/>
- <https://stackoverflow.com/questions/17110567/converting-byte-array-to-int24>

As well as a fair bit of experimentation with Audacity, ffmpeg, and a hex editor to figure out how WAV files *actually need to be parsed*.

## License

This library is licensed under the MIT License. See the included LICENSE file for details.