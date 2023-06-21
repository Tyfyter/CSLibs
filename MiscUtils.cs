using EpikV2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.IO;

namespace Tyfyter.Utils {
	public static class MiscUtils {
		public static void Unload() {
			_sortMode = null;
			_customEffect = null;
			_transformMatrix = null;
			_uImage_Armor = null;
		}
		public record SpriteBatchState(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix transformMatrix = default);
		private static FastFieldInfo<SpriteBatch, SpriteSortMode> _sortMode;
		internal static FastFieldInfo<SpriteBatch, SpriteSortMode> sortMode => _sortMode ??= new("sortMode", BindingFlags.NonPublic | BindingFlags.Instance);
		private static FastFieldInfo<SpriteBatch, Effect> _customEffect;
		internal static FastFieldInfo<SpriteBatch, Effect> customEffect => _customEffect ??= new("customEffect", BindingFlags.NonPublic | BindingFlags.Instance);
		private static FastFieldInfo<SpriteBatch, Matrix> _transformMatrix;
		internal static FastFieldInfo<SpriteBatch, Matrix> transformMatrix => _transformMatrix ??= new("transformMatrix", BindingFlags.NonPublic | BindingFlags.Instance);
		public static SpriteBatchState GetState(this SpriteBatch spriteBatch) {
			return new SpriteBatchState(
				sortMode.GetValue(spriteBatch),
				spriteBatch.GraphicsDevice.BlendState,
				spriteBatch.GraphicsDevice.SamplerStates[0],
				spriteBatch.GraphicsDevice.DepthStencilState,
				spriteBatch.GraphicsDevice.RasterizerState,
				customEffect.GetValue(spriteBatch),
				transformMatrix.GetValue(spriteBatch)
			);
		}
		public static void Restart(this SpriteBatch spriteBatch, SpriteBatchState spriteBatchState, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null) {
			spriteBatch.End();
			spriteBatch.Begin(sortMode, blendState ?? spriteBatchState.blendState, samplerState ?? spriteBatchState.samplerState, spriteBatchState.depthStencilState, rasterizerState ?? spriteBatchState.rasterizerState, effect ?? spriteBatchState.effect, transformMatrix ?? spriteBatchState.transformMatrix);
		}
		internal static FastFieldInfo<ArmorShaderData, Asset<Texture2D>> _uImage_Armor;
		internal static FastFieldInfo<MiscShaderData, Asset<Texture2D>> _uImage_Misc;
		public static void UseNonVanillaImage(this ArmorShaderData shaderData, Asset<Texture2D> texture) {
			(_uImage_Armor ??= new("_uImage", BindingFlags.NonPublic, true)).SetValue(shaderData, texture);
		}
		public static void UseNonVanillaImage(this MiscShaderData shaderData, Asset<Texture2D> texture) {
			(_uImage_Misc ??= new("_uImage", BindingFlags.NonPublic, true)).SetValue(shaderData, texture);
		}
		public static T[] BuildArray<T>(int length, params int[] nonNullIndeces) where T : new() {
			T[] o = new T[length];
			for (int i = 0; i < nonNullIndeces.Length; i++) {
				if (nonNullIndeces[i] == -1) {
					for (i = 0; i < o.Length; i++) {
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
		/// <summary>
		/// Multiplies a vector by a matrix
		/// </summary>
		/// <param name="x">the vector used to determine the x component of the output</param>
		/// <param name="y">the vector used to determine the y component of the output</param>
		public static Vector2 MatrixMult(this Vector2 value, Vector2 x, Vector2 y) {
			return new Vector2(Vector2.Dot(value, x), Vector2.Dot(value, y));
		}
		public static T SafeGet<T>(this TagCompound self, string key) {
			return self.TryGet(key, out T output) ? output : default;
		}
		public class MirrorDictionary<T> : IDictionary<T, T> {
			public T this[T key] {
				get {
					return key;
				}
				set { }
			}

			public ICollection<T> Keys { get; }
			public ICollection<T> Values { get; }
			public int Count { get; }
			public bool IsReadOnly => true;
			public void Add(T key, T value) {
				throw new NotImplementedException();
			}

			public void Add(KeyValuePair<T, T> item) {
				throw new NotImplementedException();
			}

			public void Clear() {
				throw new NotImplementedException();
			}

			public bool Contains(KeyValuePair<T, T> item) => item.Value.Equals(item.Key);
			public bool ContainsKey(T key) => true;

			public void CopyTo(KeyValuePair<T, T>[] array, int arrayIndex) {
				throw new NotImplementedException();
			}
			public IEnumerator<KeyValuePair<T, T>> GetEnumerator() {
				throw new NotImplementedException();
			}
			public bool Remove(T key) {
				throw new NotImplementedException();
			}
			public bool Remove(KeyValuePair<T, T> item) {
				throw new NotImplementedException();
			}
			public bool TryGetValue(T key, [MaybeNullWhen(false)] out T value) {
				value = key;
				return true;
			}
			IEnumerator IEnumerable.GetEnumerator() {
				throw new NotImplementedException();
			}
		}
		public struct PlayerShaderSet {
			public int cHead;
			public int cBody;
			public int cLegs;
			public int cHandOn;
			public int cHandOff;
			public int cBack;
			public int cFront;
			public int cShoe;
			public int cWaist;
			public int cShield;
			public int cNeck;
			public int cFace;
			public int cFaceHead;
			public int cFaceFlower;
			public int cBalloon;
			public int cWings;
			public int cBalloonFront;
			public int cCarpet;
			public int cFloatingTube;
			public int cBackpack;
			public int cTail;
			public int cShieldFallback;
			public int cGrapple;
			public int cMount;
			public int cMinecart;
			public int cPet;
			public int cLight;
			public int cYorai;
			public int cPortalbeStool;
			public int cUnicornHorn;
			public int cAngelHalo;
			public int cBeard;
			public int cMinion;
			public int cLeinShampoo;
			public PlayerShaderSet(Player player) {
				cHead = player.cHead;
				cBody = player.cBody;
				cLegs = player.cLegs;
				cHandOn = player.cHandOn;
				cHandOff = player.cHandOff;
				cBack = player.cBack;
				cFront = player.cFront;
				cShoe = player.cShoe;
				cWaist = player.cWaist;
				cShield = player.cShield;
				cNeck = player.cNeck;
				cFace = player.cFace;
				cFaceHead = player.cFaceHead;
				cFaceFlower = player.cFaceFlower;
				cBalloon = player.cBalloon;
				cWings = player.cWings;
				cBalloonFront = player.cBalloonFront;
				cCarpet = player.cCarpet;
				cFloatingTube = player.cFloatingTube;
				cBackpack = player.cBackpack;
				cTail = player.cTail;
				cShieldFallback = player.cShieldFallback;
				cGrapple = player.cGrapple;
				cMount = player.cMount;
				cMinecart = player.cMinecart;
				cPet = player.cPet;
				cLight = player.cLight;
				cYorai = player.cYorai;
				cPortalbeStool = player.cPortalbeStool;
				cUnicornHorn = player.cUnicornHorn;
				cAngelHalo = player.cAngelHalo;
				cBeard = player.cBeard;
				cMinion = player.cMinion;
				cLeinShampoo = player.cLeinShampoo;
			}
			public PlayerShaderSet(int shader) {
				cHead = shader;
				cBody = shader;
				cLegs = shader;
				cHandOn = shader;
				cHandOff = shader;
				cBack = shader;
				cFront = shader;
				cShoe = shader;
				cWaist = shader;
				cShield = shader;
				cNeck = shader;
				cFace = shader;
				cFaceHead = shader;
				cFaceFlower = shader;
				cBalloon = shader;
				cWings = shader;
				cBalloonFront = shader;
				cCarpet = shader;
				cFloatingTube = shader;
				cBackpack = shader;
				cTail = shader;
				cShieldFallback = shader;
				cGrapple = shader;
				cMount = shader;
				cMinecart = shader;
				cPet = shader;
				cLight = shader;
				cYorai = shader;
				cPortalbeStool = shader;
				cUnicornHorn = shader;
				cAngelHalo = shader;
				cBeard = shader;
				cMinion = shader;
				cLeinShampoo = shader;
			}
			public void Apply(Player player) {
				player.cHead = cHead;
				player.cBody = cBody;
				player.cLegs = cLegs;
				player.cHandOn = cHandOn;
				player.cHandOff = cHandOff;
				player.cBack = cBack;
				player.cFront = cFront;
				player.cShoe = cShoe;
				player.cWaist = cWaist;
				player.cShield = cShield;
				player.cNeck = cNeck;
				player.cFace = cFace;
				player.cFaceHead = cFaceHead;
				player.cFaceFlower = cFaceFlower;
				player.cBalloon = cBalloon;
				player.cWings = cWings;
				player.cBalloonFront = cBalloonFront;
				player.cCarpet = cCarpet;
				player.cFloatingTube = cFloatingTube;
				player.cBackpack = cBackpack;
				player.cTail = cTail;
				player.cShieldFallback = cShieldFallback;
				player.cGrapple = cGrapple;
				player.cMount = cMount;
				player.cMinecart = cMinecart;
				player.cPet = cPet;
				player.cLight = cLight;
				player.cYorai = cYorai;
				player.cPortalbeStool = cPortalbeStool;
				player.cUnicornHorn = cUnicornHorn;
				player.cAngelHalo = cAngelHalo;
				player.cBeard = cBeard;
				player.cMinion = cMinion;
				player.cLeinShampoo = cLeinShampoo;

			}
		}
	}
}