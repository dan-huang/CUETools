﻿using System;
using CUETools.Codecs;

namespace CUETools.DSP.Mixer
{
    public class MixingWriter : IAudioDest
    {
        private MixingSource mixer;
        private int iSource;
        private long samplePos;
        private MixingBuffer mixbuff;
        private float volume;
        private AudioEncoderSettings m_settings;

        public long Position
        {
            get { return samplePos; }
            set { throw new NotSupportedException(); }
        }

        public long FinalSampleCount
        {
            set { throw new NotSupportedException(); }
        }

        public AudioEncoderSettings Settings
        {
            get
            {
                return m_settings;
            }
        }

        public float Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        public string Path { get { return ""; } }

        public MixingWriter(MixingSource mixer, int iSource)
        {
            this.m_settings = new AudioEncoderSettings(mixer.PCM);
            this.mixer = mixer;
            this.iSource = iSource;
            this.samplePos = 0;
            this.mixbuff = null;
            this.volume = 1.0f;
        }

        public void Close()
        {
        }

        public void Delete()
        {
            Close();
        }

        public void Pause()
        {
            mixer.LockEmptyBuffer(iSource);
        }

        public void Flush()
        {
            if (mixbuff != null)
            {
                if (mixbuff.source[iSource].Length < mixbuff.source[iSource].Size)
                    AudioSamples.MemSet(mixbuff.source[iSource].Bytes, 0, mixbuff.source[iSource].Length * Settings.PCM.BlockAlign, (mixbuff.source[iSource].Size - mixbuff.source[iSource].Length) * Settings.PCM.BlockAlign);
                mixer.UnlockEmptyBuffer(mixbuff, iSource, volume);
                mixbuff = null;
            }
        }

        public void Write(AudioBuffer buff)
        {
            int bs = Settings.PCM.BlockAlign;
            int buff_offs = 0;

            while (buff_offs < buff.Length)
            {
                if (mixbuff == null)
                {
                    mixbuff = mixer.LockEmptyBuffer(iSource);
                    mixbuff.source[iSource].Prepare(-1);
                    mixbuff.source[iSource].Length = 0;
                }

                int chunk = Math.Min(buff.Length - buff_offs, mixbuff.source[iSource].Size - mixbuff.source[iSource].Length);
                Buffer.BlockCopy(buff.Float, buff_offs * bs, mixbuff.source[iSource].Float, mixbuff.source[iSource].Length * bs, chunk * bs);
                mixbuff.source[iSource].Length += chunk;
                buff_offs += chunk;

                if (mixbuff.source[iSource].Length == mixbuff.source[iSource].Size)
                {
                    mixer.UnlockEmptyBuffer(mixbuff, iSource, volume);
                    mixbuff = null;
                }
            }

            samplePos += buff.Length;
        }
    }
}
