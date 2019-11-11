﻿using GLTF.Extensions;
using Newtonsoft.Json;
using System;
using Color = UnityEngine.Color;

namespace GLTF.Schema
{
    public class MaterialCommonConstant : GLTFProperty
    {
        /// <summary>
        /// Used to scale the ambient light contributions to this material
        /// </summary>
        public Color AmbientFactor = Color.white;

        /// <summary>
        /// Texture used to store pre-computed direct lighting
        /// </summary>
        public TextureInfo LightmapTexture;

        /// <summary>
        /// Scale factor for the lightmap texture
        /// </summary>
        public Color LightmapFactor = Color.white;

        public MaterialCommonConstant()
        {
        }

        public MaterialCommonConstant(MaterialCommonConstant materialCommonConstant, GLTFRoot gltfRoot) : base(materialCommonConstant)
        {
            if (materialCommonConstant == null) return;

            AmbientFactor = materialCommonConstant.AmbientFactor;
            LightmapTexture = new TextureInfo(materialCommonConstant.LightmapTexture, gltfRoot);
            LightmapFactor = materialCommonConstant.LightmapFactor;
        }

        public static MaterialCommonConstant Deserialize(GLTFRoot root, JsonReader reader)
        {
            var commonConstant = new MaterialCommonConstant();

            if (reader.Read() && reader.TokenType != JsonToken.StartObject)
            {
                throw new Exception("Asset must be an object.");
            }

            while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
            {
                var curProp = reader.Value.ToString();

                switch (curProp)
                {
                    case "ambientFactor":
                        commonConstant.AmbientFactor = reader.ReadAsRGBColor();
                        break;
                    case "lightmapTexture":
                        commonConstant.LightmapTexture = TextureInfo.Deserialize(root, reader);
                        break;
                    case "lightmapFactor":
                        commonConstant.LightmapFactor = reader.ReadAsRGBColor();
                        break;
                    default:
                        commonConstant.DefaultPropertyDeserializer(root, reader);
                        break;
                }
            }

            return commonConstant;
        }

        public override void Serialize(JsonWriter writer)
        {
            writer.WriteStartObject();

            if (AmbientFactor != Color.white)
            {
                writer.WritePropertyName("ambientFactor");
                writer.WriteStartArray();
                writer.WriteValue(AmbientFactor.r);
                writer.WriteValue(AmbientFactor.g);
                writer.WriteValue(AmbientFactor.b);
                writer.WriteEndArray();
            }

            if (LightmapTexture != null)
            {
                writer.WritePropertyName("lightmapTexture");
                LightmapTexture.Serialize(writer);
            }

            if (LightmapFactor != Color.white)
            {
                writer.WritePropertyName("lightmapFactor");
                writer.WriteStartArray();
                writer.WriteValue(LightmapFactor.r);
                writer.WriteValue(LightmapFactor.g);
                writer.WriteValue(LightmapFactor.b);
                writer.WriteEndArray();
            }

            base.Serialize(writer);

            writer.WriteEndObject();
        }
    }
}
