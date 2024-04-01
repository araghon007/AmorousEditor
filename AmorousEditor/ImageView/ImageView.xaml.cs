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
    public partial class ImageView : Window
    {
        /// <summary>
        /// The amount of zoom
        /// </summary>
        float zoom = 1;

        float transX = 0;
        float transY = 0;
        float transSpeed = 5f;

        Bitmap image;
        System.Drawing.Size imageSize;
        Texture backgroundTexture = new Texture();

        public ImageView( Bitmap bitmap )
        {
            image = /*( Bitmap )*/bitmap/*.Clone()*/;
            imageSize = new System.Drawing.Size( bitmap.Width, bitmap.Height );

            InitializeComponent();

            openGLCtrl.Focus();
        }

        private void Cleanup( object s, RoutedEventArgs e, OpenGL gl )
        {
            image?.Dispose();
        }

        private void OpenGLDraw( object sender, RenderEventArgs args )
        {
            var gl = openGLCtrl.OpenGL;

            gl.Clear( OpenGL.GL_COLOR_BUFFER_BIT );
            gl.MatrixMode( MatrixMode.Projection );
            gl.LoadIdentity();
            gl.Viewport( 0, 0, Convert.ToInt16( host.ActualWidth ), Convert.ToInt16( host.ActualHeight ) );
            gl.MatrixMode( MatrixMode.Modelview );
            gl.LoadIdentity();
            float aspectRatio = /*( float )( host.ActualWidth / host.ActualHeight )*/1f;
            gl.Ortho( /*-host.ActualWidth **/ zoom * aspectRatio - 1f, host.ActualWidth * zoom * aspectRatio, /*-host.ActualHeight **/ zoom - 1f, host.ActualHeight * zoom, 1, -1 );

            // TODO: Proper zoom + pan

            //gl.Scale( zoom, zoom, 1 );
            gl.Translate( host.ActualWidth - transX * zoom, host.ActualHeight + transY * zoom, 0 );



            gl.Enable( OpenGL.GL_TEXTURE_2D );
            gl.Enable( OpenGL.GL_BLEND );
            gl.BlendFunc( BlendingSourceFactor.SourceAlpha, BlendingDestinationFactor.OneMinusSourceAlpha );

            this.DrawBackground( gl );

            gl.Disable( OpenGL.GL_TEXTURE_2D );
            gl.Disable( OpenGL.GL_BLEND );

            

            gl.Flush();
        }

        private void DrawBackground( OpenGL gl )
        {
            try
            {
                //MessageBox.Show( $"Image dimensions: {imageSize.Width}x{imageSize.Height}" );

                gl.Color( 1f, 1f, 1f );
                backgroundTexture.Bind( gl );
                gl.Begin( BeginMode.Quads );
                gl.TexCoord( 0, 0 );
                gl.Vertex( -imageSize.Width, imageSize.Height );
                gl.TexCoord( 1, 0 );
                gl.Vertex( imageSize.Width, imageSize.Height );
                gl.TexCoord( 1, 1 );
                gl.Vertex( imageSize.Width, -imageSize.Height );
                gl.TexCoord( 0, 1 );
                gl.Vertex( -imageSize.Width, -imageSize.Height );
                // Greenscreen
                /*
                gl.Color(0f, 1f, 0f);
                gl.Begin(BeginMode.Quads);
                gl.Vertex(-2000, 2000);
                gl.Vertex(2000, 2000);
                gl.Vertex(2000, -2000);
                gl.Vertex(-2000, -2000);
                */
            }
            catch /*( Exception e )*/
            {
                //MessageBox.Show( "DrawBackground error!" );
                //MessageBox.Show( e.ToString() );
            }
            gl.End();
        }

        private void openGLCtrl_OpenGLInitialized( object sender, EventArgs args )
        {
            var gl = openGLCtrl.OpenGL;
            gl.PointSize( 3f );
            gl.Disable( OpenGL.GL_DEPTH_TEST );

            backgroundTexture.Create( gl, image );

            
            Unloaded += ( s, e ) => Cleanup( s, e, gl );

        }

        // TODO: Proper zoom and pan
        private void openGLCtrl_MouseWheel( object sender, System.Windows.Forms.MouseEventArgs e )
        {
            zoom -= e.Delta / 1200f;

            if ( zoom == 0 )
            {
                zoom = 0.0001f;
            }
        }

        private void openGLCtrl_KeyDown( object sender, System.Windows.Forms.KeyEventArgs e )
        {
            switch ( e.KeyCode )
            {
                case System.Windows.Forms.Keys.W:
                    transY -= transSpeed;
                    break;
                case System.Windows.Forms.Keys.S:
                    transY += transSpeed;
                    break;
                case System.Windows.Forms.Keys.A:
                    transX -= transSpeed;
                    break;
                case System.Windows.Forms.Keys.D:
                    transX += transSpeed;
                    break;
            }

            e.Handled = true;
        }
    }
}
