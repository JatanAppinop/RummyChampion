using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LottiePlugin.UI;
using System.Collections;

namespace LottiePlugin.Tests.Runtime
{
    public class AnimatedImageTests
    {
        private AnimatedImage _animatedImage;

        [SetUp]
        public void SetUp()
        {
            GameObject go = new GameObject();
            _animatedImage = go.AddComponent<AnimatedImage>();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.Destroy(_animatedImage.gameObject);
        }

        [UnityTest]
        public IEnumerator CheckAwakeFunctionality()
        {
            yield return null;
            Assert.IsNotNull(_animatedImage.Transform);
        }

        [UnityTest]
        public IEnumerator CheckStartFunctionalityWithNoJsonAnimation()
        {
            yield return null;
            Assert.IsNull(_animatedImage.AnimationJson);
            Assert.IsNull(_animatedImage.RawImage.texture);
        }

        [UnityTest]
        public IEnumerator CheckStartFunctionalityWithJsonAnimation()
        {
            // Add your animationJson here
            yield return null;
            Assert.IsNotNull(_animatedImage.AnimationJson);
            Assert.IsNotNull(_animatedImage.RawImage.texture);
        }

        [UnityTest]
        public IEnumerator CheckPlayFunctionality()
        {
            // Add your animationJson here
            _animatedImage.Play();
            yield return null;
            Assert.IsTrue(_animatedImage.LottieAnimation.IsPlaying);
        }

        [UnityTest]
        public IEnumerator CheckStopFunctionality()
        {
            // Add your animationJson here
            _animatedImage.Play();
            yield return null;
            _animatedImage.Stop();
            yield return null;
            Assert.IsFalse(_animatedImage.LottieAnimation.IsPlaying);
        }

        [UnityTest]
        public IEnumerator CheckTextureDimensions()
        {
            // Add your animationJson here
            yield return null;
            Assert.AreEqual(_animatedImage.TextureWidth, _animatedImage.LottieAnimation.Texture.width);
            Assert.AreEqual(_animatedImage.TextureHeight, _animatedImage.LottieAnimation.Texture.height);
        }

        [UnityTest]
        public IEnumerator CheckDisposeFunctionality()
        {
            // Add your animationJson here
            yield return null;
            _animatedImage.DisposeLottieAnimation();
            Assert.IsNull(_animatedImage.LottieAnimation);
        }
    }
}
