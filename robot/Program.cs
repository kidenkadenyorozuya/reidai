using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using CsGL.OpenGL;

namespace robot
{
    class Program
    {
        public static void Main(string[] args)
        {
            Form f = createUI();
            f.Visible = true;

            Application.Run(f);
        }
        public static Form createUI()
        {
            Form top = new Form();
            top.Text = "OpenGL";
            //top.Bounds = new Rectangle(100, 100, 100 + 200, 100+200);

            OpenGLControl c = new MyGLView();
            c.BackColor = Color.White;
            c.Dock = DockStyle.Fill;
            top.Controls.Add(c);
            return top;
        }
    }

    public class MyGLView : OpenGLControl
    {
        const int STEPCYCLE = 400;   /* 手足のひと振りに要する時のフレーム数　　 */
        const int WALKCYCLE = 4000;  /* ステージ上を一周するのに要するフレーム数 */
        /*
        * 直方体を描く
        */
        static void myBox(double x, double y, double z)
        {
            //GLdouble hx = x * 0.5, hz = z * 0.5;
            double hx = x * 0.5, hz = z * 0.5;

            //Gdouble vertex[][3] = {
            double[][] vertex = new double[][]{
                new double[]{ -hx,   -y, -hz },
                new double[]{  hx,   -y, -hz },
                new double[]{  hx,  0.0, -hz },
                new double[]{ -hx,  0.0, -hz },
                new double[]{ -hx,   -y,  hz },
                new double[]{  hx,   -y,  hz },
                new double[]{  hx,  0.0,  hz },
                new double[]{ -hx,  0.0,  hz }
              };

            int[,] face = {
                        { 0, 1, 2, 3 },
                        { 1, 5, 6, 2 },
                        { 5, 4, 7, 6 },
                        { 4, 0, 3, 7 },
                        { 4, 5, 1, 0 },
                        { 3, 2, 6, 7 }
                      };

            double[][] normal = new double[][]{
                new double[]{ 0.0, 0.0,-1.0 },
                new double[]{ 1.0, 0.0, 0.0 },
                new double[]{ 0.0, 0.0, 1.0 },
                new double[]{-1.0, 0.0, 0.0 },
                new double[]{ 0.0,-1.0, 0.0 },
                new double[]{ 0.0, 1.0, 0.0 }
              };

            float[] red = { 0.8F, 0.2F, 0.2F, 1.0F };

            int i, j;
            
            unsafe {
                fixed (float* tmp = &red[0])
                {

                    //材質を設定する
                    GL.glMaterialfv(GL.GL_FRONT, GL.GL_DIFFUSE, tmp);
                    //GL.glColor3fv(tmp);
                };
            }
            
            System.Console.WriteLine(face[0, 0].ToString());

            GL.glBegin(GL.GL_QUADS);
            for (j = 0; j< 6; ++j) {
                GL.glNormal3dv(normal[j]);
                for (i = 4; --i >= 0;) {
                    GL.glVertex3dv(vertex[face[j,i]]);
                    //GL.glVertex3d(vertex[face[j, i]][0], vertex[face[j, i]][1], vertex[face[j, i]][2]);
                }
              }
              GL.glEnd();
        }

    /*
     * 腕／足
     */
    static void armleg(double girth, double length, double r1, double r2)
    {
        GL.glRotated(r1, 1.0, 0.0, 0.0);
        myBox(girth, length, girth);
        GL.glTranslated(0.0, -0.05 - length, 0.0);
        GL.glRotated(r2, 1.0, 0.0, 0.0);
        myBox(girth, length, girth);
    }

    /*
     * 地面を描く
     */
    static void myGround(double height)
    {
        float[][] ground = {
            new float[]{ 0.6F, 0.6F, 0.6F, 1.0F },
            new float[]{ 0.3F, 0.3F, 0.3F, 1.0F }
        };

      int i, j;

      GL.glBegin(GL.GL_QUADS);
      GL.glNormal3d(0.0, 1.0, 0.0);
      for (j = -5; j <= 5; ++j) {
        for (i = -5; i< 5; ++i) {
                    
            unsafe
            {
                //行と列を間違えた。
                fixed (float* tmp = &ground[(i + j) & 1][0])
                {
                    GL.glMaterialfv(GL.GL_FRONT, GL.GL_DIFFUSE, tmp);
                }
            }
            
            //GL.glColor3fv(ground[(i + j) & 1]);
            GL.glVertex3d((double) i, height, (double) j);
            GL.glVertex3d((double) i, height, (double)(j + 1));
            GL.glVertex3d((double)(i + 1), height, (double) (j + 1));
            GL.glVertex3d((double)(i + 1), height, (double) j);
        }
      }
      GL.glEnd();
    }

    /*
        * 画面表示
        */
    protected override void OnPaint(PaintEventArgs e)
    //static void display()
    {
        float[] lightpos = { 3.0F, 4.0F, 5.0F, 1.0F }; /* 光源の位置 */
        int frame = 0;                                     /* フレーム数 */

        /* STEPCYCLE に指定した枚数のフレームを描画する間に 0→1 に変化 */
        double t = (frame % STEPCYCLE) / (double)STEPCYCLE;

        /* WALKCYCLE に指定した枚数のフレームを描画する間に 0→1 に変化 */
        double s = (frame % WALKCYCLE) / (double)WALKCYCLE;

        /*
            * 以下の変数に値を設定する
        */

        double ll1 = 0.0; /* 箱男の左足の股関節の角度 */
        double ll2 = 0.0; /* 箱男の左足の膝関節の角度 */

        double rl1 = 0.0; /* 箱男の右足の股関節の角度 */
        double rl2 = 0.0; /* 箱男の右足の膝関節の角度 */

        double la1 = 0.0; /* 箱男の左腕の肩関節の角度 */
        double la2 = 0.0; /* 箱男の左腕の肘関節の角度 */

        double ra1 = 0.0; /* 箱男の右腕の肩関節の角度 */
        double ra2 = 0.0; /* 箱男の右腕の肘関節の角度 */

        double px = 0.0, pz = 0.0;      /* 箱男の位置 */
        double r = 0.0;                 /* 箱男の向き */
        double h = 0.0;                 /* 箱男の高さ */

        /* フレーム数（画面表示を行った回数）をカウントする */
        ++frame;

        /* 画面クリア */
        GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

        /* モデルビュー変換行列の初期化 */
        GL.glLoadIdentity();

        /* 光源の位置を設定 */
        GL.glLightfv(GL.GL_LIGHT0, GL.GL_POSITION, lightpos);

        /* 視点の移動（物体の方を奥に移す）*/
        GL.glTranslated(0.0, 0.0, -10.0);
        //GL.glTranslatef(-2.0f, 0.0f, -10.0f);
        /* シーンの描画 */

        /* 地面 */
        myGround(-1.8);

        /* 箱男の位置と方向 */
        GL.glTranslated(px, h, pz);
        GL.glRotated(r, 0.0, 1.0, 0.0);

        /* 頭 */
        myBox(0.20, 0.25, 0.22);

        /* 胴 */
        GL.glTranslated(0.0, -0.3, 0.0);
        myBox(0.4, 0.6, 0.3);
   
        /* 左足 */
        GL.glPushMatrix();
        GL.glTranslated(0.1, -0.65, 0.0);
        armleg(0.2, 0.4, ll1, ll2);
        GL.glPopMatrix();

        /* 右足 */
        GL.glPushMatrix();
        GL.glTranslated(-0.1, -0.65, 0.0);
        armleg(0.2, 0.4, rl1, rl2);
        GL.glPopMatrix();

        /* 左腕 */
        GL.glPushMatrix();
        GL.glTranslated(0.28, 0.0, 0.0);
        armleg(0.16, 0.4, la1, la2);
        GL.glPopMatrix();

        /* 右腕 */
        GL.glPushMatrix();
        GL.glTranslated(-0.28, 0.0, 0.0);
        armleg(0.16, 0.4, ra1, ra2);
        GL.glPopMatrix();

        GL.glFlush();
        SwapBuffer();

    }

        /// set glViewport. subclass to set frustrum...
        protected override void OnSizeChanged(EventArgs e)
        //static void resize(int w, int h)
        {
            base.OnSizeChanged(e);
            Size s = Size;

            int w = s.Width;
            int h = s.Height;
            double aspect_ratio = (double)s.Width / (double)s.Height;

            /* ウィンドウ全体をビューポートにする */
            GL.glViewport(0, 0, w, h);

            /* 透視変換行列の指定 */
            GL.glMatrixMode(GL.GL_PROJECTION);

            /* 透視変換行列の初期化 */
            GL.glLoadIdentity();
            GL.gluPerspective(30.0, aspect_ratio, 1.0, 100.0);

            /* モデルビュー変換行列の指定 */
            GL.glMatrixMode(GL.GL_MODELVIEW);

            GL.glLoadIdentity();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        //static void keyboard(unsigned char key, int x, int y)

        {
            base.OnKeyUp(e);
            // ESC か q をタイプしたら終了
            if (e.KeyCode == Keys.Escape | e.KeyCode == Keys.Q)
                Application.Exit();

        }

        /// some common initialisation
        protected override void InitGLContext()
        {
            GL.glShadeModel(GL.GL_SMOOTH);                          // Enable Smooth Shading
            //GL.glClearColor(0.0f, 0.0f, 0.0f, 0.5f);                // Black Background
            GL.glClearDepth(1.0f);                                  // Depth Buffer Setup
            GL.glEnable(GL.GL_DEPTH_TEST);                          // Enables Depth Testing
                                                                    //GL.glDepthFunc(GL.GL_RGBA | GL.GL_LIGHTING);  
            GL.glDepthFunc(GL.GL_LEQUAL);
            //GL.GL_RGBA | GL.GL_DEPTH
            // The Type Of Depth Testing To Do
            GL.glHint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_NICEST); // Really Nice Perspective Calculations
                                                                /* 初期設定 */
            GL.glClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            //glEnable(GL_DEPTH_TEST);
            GL.glEnable(GL.GL_CULL_FACE);
            GL.glEnable(GL.GL_LIGHTING);
            GL.glEnable(GL.GL_LIGHT0);

        }

    }
}
