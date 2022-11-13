﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectFactories;

namespace Celeste.Mod.CelesteNet {
    public class RingBuffer<T> {

        private readonly T[] Data;

        public int Length => Data.Length;
        public int Position { get; private set; }
        public int Moved { get; private set; }
        public int Start => Math.Max(Moved - Length, 0);

        public T this[int offs] {
            get => Data[(Position + (offs % Length) + Length) % Length];
            set => Data[(Position + (offs % Length) + Length) % Length] = value;
        }

        public RingBuffer(int size) {
            Data = new T[size];
        }

        public RingBuffer<T> Move(int offs) {
            offs = (offs + Length) % Length;
            Position = (Position + offs + Length) % Length;
            Moved += offs;
            return this;
        }

        public T Get() => Data[Position];
        public RingBuffer<T> Set(T value) {
            Data[Position] = value;
            return this;
        }

    }
}
