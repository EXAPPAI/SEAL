﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Research.SEAL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SEALNetTest
{
    [TestClass]
    public class CKKSEncoderTests
    {
        [TestMethod]
        public void EncodeDecodeDoubleTest()
        {
            EncryptionParameters parms = new EncryptionParameters(SchemeType.CKKS);
            parms.PolyModulusDegree = 64;
            List<SmallModulus> coeffModulus = new List<SmallModulus>(4);
            coeffModulus.Add(DefaultParams.SmallMods40Bit(0));
            coeffModulus.Add(DefaultParams.SmallMods40Bit(1));
            coeffModulus.Add(DefaultParams.SmallMods40Bit(2));
            coeffModulus.Add(DefaultParams.SmallMods40Bit(3));
            parms.CoeffModulus = coeffModulus;
            SEALContext context = SEALContext.Create(parms);

            int slots = 16;
            Plaintext plain = new Plaintext();
            double delta = 1 << 16;
            List<Complex> result = new List<Complex>();

            CKKSEncoder encoder = new CKKSEncoder(context);
            Assert.AreEqual(32ul, encoder.SlotCount);

            double value = 10d;
            encoder.Encode(value, delta, plain);
            encoder.Decode(plain, result);

            for (int i = 0; i < slots; i++)
            {
                double tmp = Math.Abs(value - result[i].Real);
                Assert.IsTrue(tmp < 0.5);
            }
        }

        [TestMethod]
        public void EncodeDecodeUlongTest()
        {

            int slots = 32;
            EncryptionParameters parms = new EncryptionParameters(SchemeType.CKKS);
            parms.PolyModulusDegree = (ulong)slots * 2;
            List<SmallModulus> coeffModulus = new List<SmallModulus>(4);
            coeffModulus.Add(DefaultParams.SmallMods40Bit(0));
            coeffModulus.Add(DefaultParams.SmallMods40Bit(1));
            coeffModulus.Add(DefaultParams.SmallMods40Bit(2));
            coeffModulus.Add(DefaultParams.SmallMods40Bit(3));
            parms.CoeffModulus = coeffModulus;
            SEALContext context = SEALContext.Create(parms);
            CKKSEncoder encoder = new CKKSEncoder(context);

            Plaintext plain = new Plaintext();
            List<Complex> result = new List<Complex>();

            ulong value = 15ul;
            encoder.Encode(value, plain);
            encoder.Decode(plain, result);

            for (int i = 0; i < slots; i++)
            {
                double tmp = Math.Abs(value - result[i].Real);
                Assert.IsTrue(tmp < 0.5);
            }
        }

        [TestMethod]
        public void EncodeDecodeComplexTest()
        {
            EncryptionParameters parms = new EncryptionParameters(SchemeType.CKKS)
            {
                PolyModulusDegree = 64,
                CoeffModulus = new List<SmallModulus>()
                {
                    DefaultParams.SmallMods40Bit(0),
                    DefaultParams.SmallMods40Bit(1),
                    DefaultParams.SmallMods40Bit(2),
                    DefaultParams.SmallMods40Bit(3)
                }
            };

            SEALContext context = SEALContext.Create(parms);
            CKKSEncoder encoder = new CKKSEncoder(context);

            Plaintext plain = new Plaintext();
            Complex value = new Complex(3.1415, 2.71828);

            encoder.Encode(value, scale: Math.Pow(2, 20), destination: plain);

            List<Complex> result = new List<Complex>();
            encoder.Decode(plain, result);

            Assert.IsTrue(result.Count > 0);
            Assert.AreEqual(3.1415, result[0].Real, delta: 0.0001);
            Assert.AreEqual(2.71828, result[0].Imaginary, delta: 0.0001);
        }

        [TestMethod]
        public void EncodeDecodeVectorTest()
        {
            int slots = 32;
            EncryptionParameters parms = new EncryptionParameters(SchemeType.CKKS);
            parms.PolyModulusDegree = (ulong)slots * 2;
            List<SmallModulus> coeffModulus = new List<SmallModulus>(4);
            coeffModulus.Add(DefaultParams.SmallMods60Bit(0));
            coeffModulus.Add(DefaultParams.SmallMods60Bit(1));
            coeffModulus.Add(DefaultParams.SmallMods60Bit(2));
            coeffModulus.Add(DefaultParams.SmallMods60Bit(3));
            parms.CoeffModulus = coeffModulus;
            SEALContext context = SEALContext.Create(parms);
            CKKSEncoder encoder = new CKKSEncoder(context);

            List<Complex> values = new List<Complex>(slots);
            Random rnd = new Random();
            int dataBound = 1 << 30;
            double delta = 1ul << 40;

            for (int i = 0; i < slots; i++)
            {
                values.Add(new Complex(rnd.Next() % dataBound, 0));
            }

            Plaintext plain = new Plaintext();
            encoder.Encode(values, delta, plain);

            List<Complex> result = new List<Complex>();
            encoder.Decode(plain, result);

            for (int i = 0; i < slots; i++)
            {
                double tmp = Math.Abs(values[i].Real - result[i].Real);
                Assert.IsTrue(tmp < 0.5);
            }
        }

        [TestMethod]
        public void EncodeDecodeVectorDoubleTest()
        {
            EncryptionParameters parms = new EncryptionParameters(SchemeType.CKKS)
            {
                PolyModulusDegree = 64,
                CoeffModulus = new List<SmallModulus>()
                {
                    DefaultParams.SmallMods30Bit(0),
                    DefaultParams.SmallMods30Bit(1)
                }
            };

            SEALContext context = SEALContext.Create(parms);
            CKKSEncoder encoder = new CKKSEncoder(context);
            Plaintext plain = new Plaintext();

            double[] values = new double[] { 0.1, 2.3, 34.4 };
            encoder.Encode(values, scale: Math.Pow(2, 20), destination: plain);

            List<double> result = new List<double>();
            encoder.Decode(plain, result);

            Assert.IsNotNull(result);
            Assert.AreEqual(0.1, result[0], delta: 0.001);
            Assert.AreEqual(2.3, result[1], delta: 0.001);
            Assert.AreEqual(34.4, result[2], delta: 0.001);
        }

        [TestMethod]
        public void ExceptionsTest()
        {
            EncryptionParameters parms = new EncryptionParameters(SchemeType.CKKS)
            {
                PolyModulusDegree = 64,
                CoeffModulus = new List<SmallModulus>()
                {
                    DefaultParams.SmallMods30Bit(0),
                    DefaultParams.SmallMods30Bit(1)
                }
            };

            SEALContext context = SEALContext.Create(parms);
            CKKSEncoder encoder = new CKKSEncoder(context);
            List<double> vald = new List<double>();
            List<double> vald_null = null;
            List<Complex> valc = new List<Complex>();
            List<Complex> valc_null = null;
            Plaintext plain = new Plaintext();
            Plaintext plain_null = null;
            MemoryPoolHandle pool = MemoryManager.GetPool(MMProfOpt.ForceGlobal);
            Complex complex = new Complex(1, 2);

            Assert.ThrowsException<ArgumentNullException>(() => encoder = new CKKSEncoder(null));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(vald, ParmsId.Zero, 10.0, plain_null));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(vald, null, 10.0, plain));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(vald_null, ParmsId.Zero, 10.0, plain));
            Assert.ThrowsException<ArgumentException>(() => encoder.Encode(vald, ParmsId.Zero, 10.0, plain, pool));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(valc, ParmsId.Zero, 10.0, plain_null));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(valc, null, 10.0, plain));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(valc_null, ParmsId.Zero, 10.0, plain));
            Assert.ThrowsException<ArgumentException>(() => encoder.Encode(valc, ParmsId.Zero, 10.0, plain, pool));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(vald, 10.0, plain_null));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(vald_null, 10.0, plain));
            Assert.ThrowsException<ArgumentException>(() => encoder.Encode(vald, -10.0, plain, pool));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(valc, 10.0, plain_null));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(valc_null, 10.0, plain));
            Assert.ThrowsException<ArgumentException>(() => encoder.Encode(valc, -10.0, plain, pool));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(10.0, ParmsId.Zero, 20.0, plain_null));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(10.0, null, 20.0, plain));
            Assert.ThrowsException<ArgumentException>(() => encoder.Encode(10.0, ParmsId.Zero, 20.0, plain, pool));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(10.0, 20.0, plain_null));
            Assert.ThrowsException<ArgumentException>(() => encoder.Encode(10.0, -20.0, plain, pool));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(complex, ParmsId.Zero, 10.0, plain_null));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(complex, null, 10.0, plain));
            Assert.ThrowsException<ArgumentException>(() => encoder.Encode(complex, ParmsId.Zero, 10.0, plain, pool));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(complex, 10.0, plain_null));
            Assert.ThrowsException<ArgumentException>(() => encoder.Encode(complex, -10.0, plain, pool));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(10ul, ParmsId.Zero, plain_null));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(10ul, null, plain));
            Assert.ThrowsException<ArgumentException>(() => encoder.Encode(10ul, ParmsId.Zero, plain));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Encode(10ul, plain_null));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Decode(plain, vald_null));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Decode(plain_null, vald));
            Assert.ThrowsException<ArgumentException>(() => encoder.Decode(plain, vald, pool));

            Assert.ThrowsException<ArgumentNullException>(() => encoder.Decode(plain, valc_null));
            Assert.ThrowsException<ArgumentNullException>(() => encoder.Decode(plain_null, valc));
            Assert.ThrowsException<ArgumentException>(() => encoder.Decode(plain, valc, pool));
        }
    }
}
