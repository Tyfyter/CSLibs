using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Terraria;

namespace Tyfyter.Utils {
	public static class MiscUtils {
        public static void Unload() {
            _sortMode = null;
            _customEffect = null;
            _transformMatrix = null;
        }
        public record SpriteBatchState(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix transformMatrix = default);
        private static FieldInfo _sortMode;
        internal static FieldInfo sortMode => _sortMode ??= typeof(SpriteBatch).GetField("sortMode", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo _customEffect;
        internal static FieldInfo customEffect => _customEffect ??= typeof(SpriteBatch).GetField("customEffect", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo _transformMatrix;
        internal static FieldInfo transformMatrix => _transformMatrix ??= typeof(SpriteBatch).GetField("transformMatrix", BindingFlags.NonPublic | BindingFlags.Instance);
        public static SpriteBatchState GetState(this SpriteBatch spriteBatch) {
            return new SpriteBatchState(
                (SpriteSortMode)sortMode.GetValue(spriteBatch),
                spriteBatch.GraphicsDevice.BlendState,
                spriteBatch.GraphicsDevice.SamplerStates[0],
                spriteBatch.GraphicsDevice.DepthStencilState,
                spriteBatch.GraphicsDevice.RasterizerState,
                (Effect)customEffect.GetValue(spriteBatch),
                (Matrix)transformMatrix.GetValue(spriteBatch)
            );
        }
        public static void Restart(this SpriteBatch spriteBatch, SpriteBatchState spriteBatchState, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null) {
            spriteBatch.End();
            spriteBatch.Begin(sortMode, blendState ?? spriteBatchState.blendState, samplerState ?? spriteBatchState.samplerState, spriteBatchState.depthStencilState, rasterizerState ?? spriteBatchState.rasterizerState, effect ?? spriteBatchState.effect, transformMatrix ?? spriteBatchState.transformMatrix);
        }
        public static T[] BuildArray<T>(int length, params int[] nonNullIndeces) where T : new() {
            T[] o = new T[length];
            for(int i = 0; i < nonNullIndeces.Length; i++) {
                if(nonNullIndeces[i] == -1) {
                    for(i = 0; i < o.Length; i++) {
                        o[i] = new T();
                    }
                    break;
                }
                o[nonNullIndeces[i]] = new T();
            }
            return o;
        }
        public struct AccumulatingAverage {
            double sum;
            int count;
            public void Add(double value) {
                sum += value;
                count++;
            }
            public static AccumulatingAverage Add(AccumulatingAverage average, double value) {
                average.Add(value);
                return average;
            }
            public static explicit operator double(AccumulatingAverage value) {
                return value.sum / value.count;
            }
            public static explicit operator int(AccumulatingAverage value) {
                return (int)(value.sum / value.count);
            }
            public static explicit operator string(AccumulatingAverage value) {
                return $"{value.sum / value.count}";
            }
        }
    }
}