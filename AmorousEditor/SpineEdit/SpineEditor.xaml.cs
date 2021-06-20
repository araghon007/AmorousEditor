using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Drawing;
using SharpGL;
using SharpGL.Enumerations;
using Spine;
using System.IO;
using SharpGL.SceneGraph.Assets;

namespace AmorousEditor
{
    /// <summary>
    /// Interaction logic for SpineEditor.xaml
    /// </summary>
    public partial class SpineEditor : Window
    {
        /// <summary>
        /// The amount of zoom
        /// </summary>
        float zoom = 1;

        Skeleton skelet;

        Stopwatch sw = new Stopwatch();

        AnimationState anim;

        Dictionary<string, Texture> atlasTextures = new Dictionary<string, Texture>();

        Dictionary<string, Bitmap> atlases;

        Bitmap sceneBackground;

        Texture backgroundTexture = new Texture();

        bool PlayAnim = false;

        bool HasBackground = false;

        public SpineEditor(TextReader atlas, string atlasPath, TextReader json, Dictionary<string, Bitmap> bitmaps, Bitmap background = null)
        {
            TextureLoader textureLoader = new FakeTextureLoad();

            atlases = bitmaps;

            var skeljs = new SkeletonJson(new Atlas(atlas, atlasPath, textureLoader));
            
            skelet = new Skeleton(skeljs.ReadSkeletonData(json));
            if (background != null)
            {
                sceneBackground = background;
                HasBackground = true;
            }


            skelet.SetSkin(skelet.Data.DefaultSkin);

            skelet.SetSlotsToSetupPose();

            skelet.UpdateWorldTransform();


            InitializeComponent();

            foreach (var skin in skelet.Data.Skins)
            {
                SkinSelect.Items.Add(skin.Name);
                skelet.SetSkin(skin.Name);
            }

            anim = new AnimationState(new AnimationStateData(skelet.Data));
            
            anim.SetAnimation(0, skelet.Data.Animations.FirstOrDefault(), true);

        }

        private void Cleanup(object s, RoutedEventArgs e, OpenGL gl)
        {
            anim.ClearTracks();

            foreach(var text in atlasTextures)
            {
                text.Value.Destroy(gl);
            }

            e.Handled = true;
        }
        
        private void OpenGLDraw(object sender, RenderEventArgs args)
        {
            var gl = openGLCtrl.OpenGL;

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT);
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadIdentity();
            gl.Viewport(0, 0, Convert.ToInt16(host.ActualWidth), Convert.ToInt16(host.ActualHeight));
            gl.MatrixMode(MatrixMode.Modelview);
            gl.LoadIdentity();
            gl.Ortho(0, host.ActualWidth, -host.ActualHeight, 0, 1, -1);

            // TODO: Proper zoom + pan
            gl.Translate(host.ActualWidth/2, -host.ActualHeight, 0);
            
            gl.Scale(zoom, zoom, 1);

            //Animations
            if (PlayAnim)
            {
                anim.Update((float)sw.Elapsed.TotalSeconds);
                anim.Apply(skelet);
                skelet.UpdateWorldTransform();
            }



            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(BlendingSourceFactor.SourceAlpha, BlendingDestinationFactor.OneMinusSourceAlpha);

            DrawBackground(gl);
            foreach (var slot in skelet.DrawOrder)
            {
                if (slot.Attachment != null && !slot.Attachment.Name.Contains("horse") && !slot.Attachment.Name.Contains("Shorts") && !slot.Attachment.Name.Contains("Remy") && !slot.Attachment.Name.Contains("sleeve") && !slot.Attachment.Name.Contains("stripes") && !slot.Attachment.Name.Contains("underbelly") && !slot.Attachment.Name.Contains("Boobs") && !slot.Attachment.Name.Contains("Nipples") && !slot.Attachment.Name.Contains("fold") && !slot.Attachment.Name.Contains("Blink") && !slot.Attachment.Name.Contains("Security"))
                {
                    if (slot.Attachment.GetType() == typeof(RegionAttachment))
                    {
                        var worldVertices = new float[2048];
                        var mesh = (RegionAttachment)slot.Attachment;
                        mesh.ComputeWorldVertices(slot.Bone, worldVertices);

                        gl.Color(1f, 1f, 1f);
                        atlasTextures[(mesh.RendererObject as AtlasRegion).page.name].Bind(gl);
                        gl.Begin(BeginMode.Quads);
                        for (int i = 0; i < 8; i += 2)
                        {
                            gl.TexCoord(mesh.UVs[i], mesh.UVs[i + 1]);
                            gl.Vertex(worldVertices[i], worldVertices[i + 1]);
                        }
                        gl.End();
                    }
                    else if (slot.Attachment.GetType() == typeof(MeshAttachment))
                    {
                        var worldVertices = new float[2048];
                        var mesh = (MeshAttachment)slot.Attachment;
                        mesh.ComputeWorldVertices(slot, worldVertices);
                        atlasTextures[(mesh.RendererObject as AtlasRegion).page.name].Bind(gl);

                        //Testing coloring different body parts, will be used later
                        
                        gl.Color(1f, 1f, 1f);
                        
                        gl.Begin(BeginMode.Triangles);
                        for (var i = 0; i < mesh.Triangles.Length; ++i)
                        {
                            var index = mesh.Triangles[i] << 1;
                            gl.TexCoord(mesh.UVs[index], mesh.UVs[index + 1]);
                            gl.Vertex(worldVertices[index], worldVertices[index + 1]);
                        }
                        gl.End();
                    }

                }
            }
            
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_BLEND);
            
            /* Debug bone drawing
            foreach (var bone in skelet.Bones)
            {
                if (bone.Parent != null)
                {
                    gl.Begin(BeginMode.Lines);
                    gl.Color(0f, 1f, 1f);
                    gl.Vertex(bone.WorldX, bone.WorldY);
                    gl.Vertex(bone.WorldX + Math.Cos(bone.WorldRotationX * (Math.PI / 180)) * bone.Data.Length, bone.WorldY + Math.Sin(bone.WorldRotationX * (Math.PI / 180)) * bone.Data.Length);
                    gl.End();
                }
                
                gl.Begin(BeginMode.Points);
                gl.Color(1f, 1f, 0f);
                gl.Vertex(bone.WorldX, bone.WorldY);
                gl.End();
                gl.DrawText(Convert.ToInt16(bone.WorldX * zoom) + Convert.ToInt16(host.ActualWidth / 2), Convert.ToInt16(bone.WorldY * zoom), 1f, 1f, 1f, "Arial", 12f, bone.Data.Name);
                
            }
            */

            gl.Flush();
            

            sw.Restart();
        }

        private void DrawBackground(OpenGL gl)
        {
            gl.Color(1f, 1f, 1f);
            backgroundTexture.Bind(gl);
            gl.Begin(BeginMode.Quads);
            gl.TexCoord(0, 0);
            gl.Vertex(-960, 1080);
            gl.TexCoord(1, 0);
            gl.Vertex(960, 1080);
            gl.TexCoord(1, 1);
            gl.Vertex(960, 0);
            gl.TexCoord(0, 1);
            gl.Vertex(-960, 0);
            /* Greenscreen
            gl.Color(0f, 1f, 0f);
            gl.Begin(BeginMode.Quads);
            gl.Vertex(-2000, 2000);
            gl.Vertex(2000, 2000);
            gl.Vertex(2000, -2000);
            gl.Vertex(-2000, -2000);
            */
            gl.End();
        }

        private void openGLCtrl_OpenGLInitialized(object sender, EventArgs args)
        {
            var gl = openGLCtrl.OpenGL;
            gl.PointSize(3f);
            sw.Start();
            gl.Disable(OpenGL.GL_DEPTH_TEST);

            if (HasBackground)
            {
                backgroundTexture.Create(gl, sceneBackground);
            }

            foreach (var file in atlases)
            {
                var text = new Texture();

                text.Create(gl, file.Value);

                atlasTextures.Add(file.Key, text);
            }
            Unloaded += (s, e) => Cleanup(s, e, gl);

        }

        // TODO: Proper zoom and pan
        private void openGLCtrl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            zoom += e.Delta / 1200f;

            if(zoom == 0)
            {
                zoom = 0.0001f;
            }
        }

        private void SelectSkin(object sender, SelectionChangedEventArgs e)
        {
            skelet.SetSkin((sender as ComboBox).SelectedItem.ToString());
            skelet.SetSlotsToSetupPose();
        }

        private void PlayAnimation(object sender, RoutedEventArgs e)
        {
            PlayAnim = (sender as CheckBox).IsChecked ?? false;
        }
    }


    /// <summary>
    /// Just a placeholder, Spine needs it to work though
    /// </summary>
    public class FakeTextureLoad : TextureLoader
    {

        public void Load(AtlasPage page, string path)
        {
        }

        public void Unload(object texture)
        {
        }
    }
}
