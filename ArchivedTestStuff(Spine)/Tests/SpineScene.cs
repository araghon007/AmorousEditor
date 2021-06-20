using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AmorousEditor
{
    public class SpineScene
    {
        [JsonProperty("skeleton", NullValueHandling = NullValueHandling.Ignore)]
        public SpineSkeleton Skeleton { get; set; }

        [JsonProperty("bones", NullValueHandling = NullValueHandling.Ignore)]
        public SpineBones[] Bones { get; set; }

        [JsonProperty("slots", NullValueHandling = NullValueHandling.Ignore)]
        public SpineBoneSlots[] Slots { get; set; }

        [JsonProperty("skins", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Dictionary<string, Dictionary<string, SpineSkinAttachments>>> Skins { get; set; }

        [JsonProperty("animations", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, SpineAnimation> Animations { get; set; }

    }
    
    public class SpineSkeleton
    {
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("spine", NullValueHandling = NullValueHandling.Ignore)]
        public string SpineVersion { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public float Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public float Height { get; set; }
    }

    public class SpineBones
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
        public string Parent { get; set; }

        [JsonProperty("length", NullValueHandling = NullValueHandling.Ignore)]
        public float Length { get; set; }

        [JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
        public float Rotation { get; set; }

        [JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
        public float X { get; set; }

        [JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
        public float Y { get; set; }
    }
    
    public class SpineBoneSlots
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("bone", NullValueHandling = NullValueHandling.Ignore)]
        public string Bone { get; set; }

        [JsonProperty("attachment", NullValueHandling = NullValueHandling.Ignore)]
        public string Attachment { get; set; }
    }

    public class SpineSkinAttachments
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty("uvs", NullValueHandling = NullValueHandling.Ignore)]
        public float[] UVs { get; set; }

        [JsonProperty("triangles", NullValueHandling = NullValueHandling.Ignore)]
        public int[] Triangles { get; set; }

        [JsonProperty("verticies", NullValueHandling = NullValueHandling.Ignore)]
        public float[] Verticies { get; set; }

        [JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
        public float X { get; set; }

        [JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
        public float Y { get; set; }

        [JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
        public float Rotation { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public int Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public int Height { get; set; }

        [JsonProperty("hull", NullValueHandling = NullValueHandling.Ignore)]
        public int Hull { get; set; }
    }

    public class SpineAnimation
    {
        [JsonProperty("slots", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, SpineAnimationSlots> Slots { get; set; }

        [JsonProperty("bones", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, SpineAnimationBone> Bones { get; set; }

        [JsonProperty("deform", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Dictionary<string, Dictionary<string, SpineAnimationDeform[]>>> Deform { get; set; }
    }

    public class SpineAnimationSlots
    {
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public SpineAnimationColor[] ColorAnim { get; set; }
    }

    public class SpineAnimationColor
    {
        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public float Time { get; set; }

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public string Color { get; set; }

        [JsonProperty("curve", NullValueHandling = NullValueHandling.Ignore)]
        public object Curve { get; set; }
    }

    public class SpineAnimationBone
    {
        [JsonProperty("rotate", NullValueHandling = NullValueHandling.Ignore)]
        public SpineAnimationRotation[] RotationAnim { get; set; }

        [JsonProperty("translate", NullValueHandling = NullValueHandling.Ignore)]
        public SpineAnimationTranslation[] TranslationAnim { get; set; }
    }

    public class SpineAnimationRotation
    {
        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public float Time { get; set; }

        [JsonProperty("angle", NullValueHandling = NullValueHandling.Ignore)]
        public float Angle { get; set; }

        [JsonProperty("curve", NullValueHandling = NullValueHandling.Ignore)]
        public object Curve { get; set; }
    }

    public class SpineAnimationTranslation
    {
        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public float Time { get; set; }

        [JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
        public float X { get; set; }

        [JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
        public float Y { get; set; }

        [JsonProperty("curve", NullValueHandling = NullValueHandling.Ignore)]
        public object Curve { get; set; }
    }

    public class SpineAnimationDeform
    {
        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public float Time { get; set; }
        
        [JsonProperty("verticies", NullValueHandling = NullValueHandling.Ignore)]
        public float[] Verticies { get; set; }

        [JsonProperty("curve", NullValueHandling = NullValueHandling.Ignore)]
        public object Curve { get; set; }
    }
}
