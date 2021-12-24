using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Lab_8
{
    public partial class Form1 : Form
    {
        // stari de control camera 
        private int eyePosX, eyePosY, eyePosZ;

        private Point mousePos;
        private float camDepth;

        // stari de control mouse 
        private bool statusControlMouse2D, statusControlMouse3D, statusMouseDown;

        // stari de control axe de coordonate 
        private bool statusControlAxe;

        // stari de control iluminare 
        private bool lightON;
        private bool lightON_0;

        /// <summary>
        /// modificare laborator 8 - ex 3 
        /// stare de control iluminare pentru cea de a doua sursa de lumina 
        /// </summary>
        private bool lightON_1;

        // stari de control obiecte 3D.
        private string statusCube;

        // stocare a vertexurilor si a listelor de vertexuri 
        private int[,] arrVertex = new int[50, 3]; // stocare matrice de vertexuri. 3 coloane pentru X, Y, Z. Nr. de linii define?te nr. de vertexuri.
        private int nVertex;

        private int[] arrQuadsList = new int[100]; // lista de vertexuri pentru construirea cubului prin intermediul quadurilor. 
        private int nQuadsList;

        private int[] arrTrianglesList = new int[100]; // lista de vertexuri pentru construirea cubului prin intermediul triunghiurilor. 
        private int nTrianglesList;

        // fisiere pentru manipularea vertexurilor
        private string fileVertex = @"./../../vertexList.txt";
        private string fileQList = @"./../../quadsVertexList.txt";
        private string fileTList = @"./../../trianglesVertexList.txt";
        private bool statusFiles;

        // dimensiune pentru valuesAmbientTemplate0() As Single = {255, 0, 0, 1.0}      
        // valori alternative ambientale (lumina colorata)
        //# SET 1
        private float[] valuesAmbientTemplate0 = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
        private float[] valuesDiffuseTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        private float[] valuesSpecularTemplate0 = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
        private float[] valuesPositionTemplate0 = new float[] { 0.0f, 0.0f, 5.0f, 1.0f };

        //# SET 2

        /// <summary>
        /// modificare laborator 8 - ex 3 
        /// valori alternative ambientale (lumina colorata) - cea de a doua sursa de lumina 
        /// </summary>
        private float[] valuesAmbientTemplate1 = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
        private float[] valuesDiffuseTemplate1 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        private float[] valuesSpecularTemplate1 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        private float[] valuesPositionTemplate1 = new float[] { 1.0f, 1.0f, 1.0f, 0.0f };

        private float[] valuesAmbient0 = new float[4];
        private float[] valuesDiffuse0 = new float[4];
        private float[] valuesSpecular0 = new float[4];
        private float[] valuesPosition0 = new float[4];

        private float[] valuesAmbient1 = new float[4];
        private float[] valuesDiffuse1 = new float[4];
        private float[] valuesSpecular1 = new float[4];
        private float[] valuesPosition1 = new float[4];

        private Axes axe;
        private Cub3D cub;

        // similar metodei OnLoad() - incarcarea resurselor 
        public Form1()
        {
            axe = new Axes();
            cub = new Cub3D();

            InitializeComponent();

            /// TODO:
            /// În fi?ierul <Form1.Designer.cs>, la linia 26 înlocui?i con?înutul original cu linia de mai jos:
            ///         this.GlControl1 = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));
            /// Acest mod de ini?ializare va activa antialiasing-ul (multisampling MSAA la 8x).
            /// ATENT?IE!
            /// Ve?i pierde designerul grafic. Aplica?ia func?ioneaz? dar pentru a putea accesa designerul grafic va trebui s? reveni?i la constructorul
            /// implicit al controlului OpenTK!

            /// s-au implementat modificarile aferente si au fost explicate in documentatia atasata pe Classroom  
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            SetupValues();
            SetupWindowGUI();
        }

        // setari initiale 
        private void SetupValues()
        {
            eyePosX = 100;
            eyePosY = 100;
            eyePosZ = 50;

            camDepth = 1.04f;

            setLight0Values();
            setLight1Values();

            numericXeye.Value = eyePosX;
            numericYeye.Value = eyePosY;
            numericZeye.Value = eyePosZ;
        }

        private void SetupWindowGUI()
        {

            setControlMouse2D(false);
            setControlMouse3D(false);

            numericCameraDepth.Value = (int)camDepth;

            setControlAxe(true);

            setCubeStatus("OFF");
            setIlluminationStatus(false);
            setSource0Status(false);
            setSource1Status(false);

            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();

            setLight1Default();
            setColorAmbientLigh1Default();
            setColorDifuseLigh1Default();
            setColorSpecularLigh1Default();
        }

        // manipulare vertexuri si liste de coordonate 
        // incarcarea coordonatelor vertexurilor si lista de compunere a obiectelor 3D. 
        private void loadVertex()
        {
            // testam daca fisierul exista 
            try
            {
                StreamReader fileReader = new StreamReader((fileVertex));
                nVertex = Convert.ToInt32(fileReader.ReadLine().Trim());
                Console.WriteLine("Vertexuri citite: " + nVertex.ToString());

                string tmpStr = "";
                string[] str = new string[3];
                for (int i = 0; i < nVertex; i++)
                {
                    tmpStr = fileReader.ReadLine();
                    str = tmpStr.Trim().Split(' ');
                    arrVertex[i, 0] = Convert.ToInt32(str[0].Trim());
                    arrVertex[i, 1] = Convert.ToInt32(str[1].Trim());
                    arrVertex[i, 2] = Convert.ToInt32(str[2].Trim());
                }
                fileReader.Close();

            }
            catch (Exception)
            {
                statusFiles = false;
                Console.WriteLine("Fisierul cu informa?ii vertex <" + fileVertex + "> nu exista!");
                MessageBox.Show("Fisierul cu informa?ii vertex <" + fileVertex + "> nu exista!");
            }
        }

        private void loadQList()
        {
            // testam daca fisierul exista 
            try
            {
                StreamReader fileReader = new StreamReader(fileQList);

                int tmp;
                string line;
                nQuadsList = 0;

                while ((line = fileReader.ReadLine()) != null)
                {
                    tmp = Convert.ToInt32(line.Trim());
                    arrQuadsList[nQuadsList] = tmp;
                    nQuadsList++;
                }

                fileReader.Close();
            }
            catch (Exception)
            {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informa?ii vertex <" + fileQList + "> nu exista!");
            }

        }

        private void loadTList()
        {
            // testam daca fisierul exista 
            try
            {
                StreamReader fileReader = new StreamReader(fileTList);

                int tmp;
                string line;
                nTrianglesList = 0;

                while ((line = fileReader.ReadLine()) != null)
                {
                    tmp = Convert.ToInt32(line.Trim());
                    arrTrianglesList[nTrianglesList] = tmp;
                    nTrianglesList++;
                }

                fileReader.Close();
            }
            catch (Exception)
            {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informa?ii vertex <" + fileTList + "> nu exista!");
            }

        }

        // control camera 
        // controlul camerei dupa axa X cu spinner numeric (un cadran). 
        private void numericXeye_ValueChanged(object sender, EventArgs e)
        {
            eyePosX = (int)numericXeye.Value;
            GlControl1.Invalidate(); //For?eaz? redesenarea întregului control OpenGL. Modific?rile vor fi luate în considerare (actualizare).
        }

        // controlul camerei dup? axa Y cu spinner numeric (un cadran).
        private void numericYeye_ValueChanged(object sender, EventArgs e)
        {
            eyePosY = (int)numericYeye.Value;
            GlControl1.Invalidate(); //For?eaz? redesenarea întregului control OpenGL. Modific?rile vor fi luate în considerare (actualizare).
        }

        // controlul camerei dup? axa Z cu spinner numeric (un cadran).
        private void numericZeye_ValueChanged(object sender, EventArgs e)
        {
            eyePosZ = (int)numericZeye.Value;
            GlControl1.Invalidate(); //For?eaz? redesenarea întregului control OpenGL. Modific?rile vor fi luate în considerare (actualizare).
        }

        // controlul adâncimii camerei fa?? de (0,0,0).
        private void numericCameraDepth_ValueChanged(object sender, EventArgs e)
        {
            camDepth = 1 + ((float)numericCameraDepth.Value) * 0.1f;
            GlControl1.Invalidate();
        }

        // control mouse 
        // setam variabila de stare pentru rotatia in 2D a mouseului.
        private void setControlMouse2D(bool status)
        {
            if (status == false)
            {
                statusControlMouse2D = false;
                btnMouseControl2D.Text = "2D mouse control OFF";
            }
            else
            {
                statusControlMouse2D = true;
                btnMouseControl2D.Text = "2D mouse control ON";
            }
        }

        //Set?m variabila de stare pentru rota?ia în 3D a mouseului.
        private void setControlMouse3D(bool status)
        {
            if (status == false)
            {
                statusControlMouse3D = false;
                btnMouseControl3D.Text = "3D mouse control OFF";
            }
            else
            {
                statusControlMouse3D = true;
                btnMouseControl3D.Text = "3D mouse control ON";
            }
        }

        //Controlul mi?c?rii setului de coordonate cu ajutorul mouseului (în plan 2D/3D)
        private void btnMouseControl2D_Click(object sender, EventArgs e)
        {
            if (statusControlMouse2D == true)
            {
                setControlMouse2D(false);
            }
            else
            {
                setControlMouse3D(false);
                setControlMouse2D(true);
            }
        }
        private void btnMouseControl3D_Click(object sender, EventArgs e)
        {
            if (statusControlMouse3D == true)
            {
                setControlMouse3D(false);
            }
            else
            {
                setControlMouse2D(false);
                setControlMouse3D(true);
            }
        }

        //Mi?carea lumii 3D cu ajutorul mouselui (click'n'drag de mouse).
        private void GlControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (statusMouseDown == true)
            {
                mousePos = new Point(e.X, e.Y);
                GlControl1.Invalidate();     //For?eaz? redesenarea întregului control OpenGL. Modific?rile vor fi luate în considerare (actualizare).
            }
        }
        private void GlControl1_MouseDown(object sender, MouseEventArgs e)
        {
            statusMouseDown = true;
        }
        private void GlControl1_MouseUp(object sender, MouseEventArgs e)
        {
            statusMouseDown = false;
        }

        // control iluminare 
        //Set?m variabila de stare pentru iluminare.
        private void setIlluminationStatus(bool status)
        {
            if (status == false)
            {
                lightON = false;
                btnLights.Text = "Iluminare OFF";
            }
            else
            {
                lightON = true;
                btnLights.Text = "Iluminare ON";
            }
        }

        //Activ?m/dezactiv?m iluminarea.
        private void btnLights_Click(object sender, EventArgs e)
        {
            if (lightON == false)
            {
                setIlluminationStatus(true);
            }
            else
            {
                setIlluminationStatus(false);
            }
            GlControl1.Invalidate();
        }

        //Identific? num?rul maxim de lumini pentru implementarea curent? a OpenGL.
        private void btnLightsNo_Click(object sender, EventArgs e)
        {
            int nr = GL.GetInteger(GetPName.MaxLights);
            MessageBox.Show("Nr. maxim de luminii pentru aceasta implementare este <" + nr.ToString() + ">.");
        }

        //Set?m variabila de stare pentru sursa de lumin? 0.
        private void setSource0Status(bool status)
        {
            if (status == false)
            {
                lightON_0 = false;
                btnLight0.Text = "Sursa 0 OFF";
            }
            else
            {
                lightON_0 = true;
                btnLight0.Text = "Sursa 0 ON";
            }
        }

        /// <summary>
        /// modificare laborator 8 - ex 3 
        /// setare variabila de stare pentru cea de a doua sursa de lumina 
        /// </summary>
        /// <param name="status"></param>
        private void setSource1Status(bool status)
        {
            if (status == false)
            {
                lightON_1 = false; // "Sursa 1 OFF";
                btnLight1.Text = "Sursa 1 OFF";
            }
            else
            {
                lightON_1 = true; // "Sursa 0 ON";
                btnLight1.Text = "Sursa 1 ON";
            }
        }

        //Activ?m/dezactiv?m sursa 0 de iluminare (doar dac? iluminarea este activ?).
        private void btnLight0_Click(object sender, EventArgs e)
        {
            if (lightON == true)
            {
                if (lightON_0 == false)
                {
                    setSource0Status(true);
                }
                else
                {
                    setSource0Status(false);
                }
                GlControl1.Invalidate();
            }
        }

        /// <summary>
        /// modificare laborator 8 - ex 3 , event handler-ul btnLight1_Click 
        /// activam / dezactivam cea de a doua sursa de iluminare (doar daca iluminarea este activa) 
        /// </summary> 
        private void btnLight1_Click(object sender, EventArgs e)
        {
            if (lightON == true)
            {
                if (lightON_1 == false)
                {
                    setSource1Status(true);
                }
                else
                {
                    setSource1Status(false);
                }
                GlControl1.Invalidate();
            }
        }

        //Schimb?m pozi?ia sursei 0 de iluminare dup? axele XYZ.
        private void setTrackLigh0Default()
        {
            trackLight0PositionX.Value = (int)valuesPosition0[0];
            trackLight0PositionY.Value = (int)valuesPosition0[1];
            trackLight0PositionZ.Value = (int)valuesPosition0[2];
        }

        /// <summary>
        /// modificare laborator 8 - ex 3 
        /// </summary>
        private void setLight1Default()
        {
            valuesPosition1[0] = 0;
            valuesPosition1[1] = 0;
            valuesPosition1[2] = 0;
        }

        private void trackLight0PositionX_Scroll(object sender, EventArgs e)
        {
            valuesPosition0[0] = trackLight0PositionX.Value;
            GlControl1.Invalidate();
        }
        private void trackLight0PositionY_Scroll(object sender, EventArgs e)
        {
            valuesPosition0[1] = trackLight0PositionY.Value;
            GlControl1.Invalidate();
        }
        private void trackLight0PositionZ_Scroll(object sender, EventArgs e)
        {
            valuesPosition0[2] = trackLight0PositionZ.Value;
            GlControl1.Invalidate();
        }

        //Schimb?m culoarea sursei de lumin? 0 (ambiental) în domeniul RGB.
        private void setColorAmbientLigh0Default()
        {
            numericLight0Ambient_Red.Value = (decimal)valuesAmbient0[0];
            numericLight0Ambient_Green.Value = (decimal)valuesAmbient0[1];
            numericLight0Ambient_Blue.Value = (decimal)valuesAmbient0[2];
        }

        /// <summary>
        /// modificare laborator 8 - ex 3 
        /// Schimb?m culoarea sursei de lumin? 1 (ambiental) în domeniul RGB.
        /// </summary>
        private void setColorAmbientLigh1Default()
        {
            numericLight0Ambient_Red.Value = (decimal)valuesAmbient1[0];
            numericLight0Ambient_Green.Value = (decimal)valuesAmbient1[1];
            numericLight0Ambient_Blue.Value = (decimal)valuesAmbient1[2];
        }

        private void numericLight0Ambient_Red_ValueChanged(object sender, EventArgs e)
        {
            valuesAmbient0[0] = (float)numericLight0Ambient_Red.Value / 100;
            valuesAmbient1[0] = (float)numericLight0Ambient_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Ambient_Green_ValueChanged(object sender, EventArgs e)
        {
            valuesAmbient0[1] = (float)numericLight0Ambient_Green.Value / 100;
            valuesAmbient1[1] = (float)numericLight0Ambient_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Ambient_Blue_ValueChanged(object sender, EventArgs e)
        {
            valuesAmbient0[2] = (float)numericLight0Ambient_Blue.Value / 100;
            valuesAmbient1[2] = (float)numericLight0Ambient_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        //Schimb?m culoarea sursei de lumin? 0 (difuz?) în domeniul RGB.
        private void setColorDifuseLigh0Default()
        {
            numericLight0Difuse_Red.Value = (decimal)valuesDiffuse0[0];
            numericLight0Difuse_Green.Value = (decimal)valuesDiffuse0[1];
            numericLight0Difuse_Blue.Value = (decimal)valuesDiffuse0[2];
        }

        /// <summary>
        /// modificare laborator 8 - ex 3 
        /// Schimb?m culoarea sursei de lumin? 1 (difuz?) în domeniul RGB.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setColorDifuseLigh1Default()
        {
            numericLight0Difuse_Red.Value = (decimal)valuesDiffuse1[0];
            numericLight0Difuse_Green.Value = (decimal)valuesDiffuse1[1];
            numericLight0Difuse_Blue.Value = (decimal)valuesDiffuse1[2];
        }

        private void numericLight0Difuse_Red_ValueChanged(object sender, EventArgs e)
        {
            valuesDiffuse0[0] = (float)numericLight0Difuse_Red.Value / 100;
            valuesDiffuse1[0] = (float)numericLight0Difuse_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Difuse_Green_ValueChanged(object sender, EventArgs e)
        {
            valuesDiffuse0[1] = (float)numericLight0Difuse_Green.Value / 100;
            valuesDiffuse1[1] = (float)numericLight0Difuse_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Difuse_Blue_ValueChanged(object sender, EventArgs e)
        {
            valuesDiffuse0[2] = (float)numericLight0Difuse_Blue.Value / 100;
            valuesDiffuse1[2] = (float)numericLight0Difuse_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        //Schimb?m culoarea sursei de lumin? 0 (specular) în domeniul RGB.
        private void setColorSpecularLigh0Default()
        {
            numericLight0Specular_Red.Value = (decimal)valuesSpecular0[0];
            numericLight0Specular_Green.Value = (decimal)valuesSpecular0[1];
            numericLight0Specular_Blue.Value = (decimal)valuesSpecular0[2];
        }

        /// <summary>
        /// modificare laborator 8 - ex 3 
        /// Schimb?m culoarea sursei de lumin? 1 (specular) în domeniul RGB.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setColorSpecularLigh1Default()
        {
            numericLight0Specular_Red.Value = (decimal)valuesSpecular1[0];
            numericLight0Specular_Green.Value = (decimal)valuesSpecular1[1];
            numericLight0Specular_Blue.Value = (decimal)valuesSpecular1[2];
        }

        private void numericLight0Specular_Red_ValueChanged(object sender, EventArgs e)
        {
            valuesSpecular0[0] = (float)numericLight0Specular_Red.Value / 100;
            valuesSpecular1[0] = (float)numericLight0Specular_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Specular_Green_ValueChanged(object sender, EventArgs e)
        {
            valuesSpecular0[1] = (float)numericLight0Specular_Green.Value / 100;
            valuesSpecular1[1] = (float)numericLight0Specular_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Specular_Blue_ValueChanged(object sender, EventArgs e)
        {
            valuesSpecular0[2] = (float)numericLight0Specular_Blue.Value / 100;
            valuesSpecular1[2] = (float)numericLight0Specular_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        //Resetare stare surs? de lumin? nr. 0.
        private void setLight0Values()
        {
            for (int i = 0; i < valuesAmbientTemplate0.Length; i++)
            {
                valuesAmbient0[i] = valuesAmbientTemplate0[i];
            }
            for (int i = 0; i < valuesDiffuseTemplate0.Length; i++)
            {
                valuesDiffuse0[i] = valuesDiffuseTemplate0[i];
            }
            for (int i = 0; i < valuesPositionTemplate0.Length; i++)
            {
                valuesPosition0[i] = valuesPositionTemplate0[i];
            }
        }

        /// <summary>
        /// modificare laborator 8 - ex 3 , resetare stare sursa de lumina nr. 1. (cea de a doua) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setLight1Values()
        {
            for (int i = 0; i < valuesAmbientTemplate1.Length; i++)
            {
                valuesAmbient1[i] = valuesAmbientTemplate1[i];
            }
            for (int i = 0; i < valuesDiffuseTemplate1.Length; i++)
            {
                valuesDiffuse1[i] = valuesDiffuseTemplate1[i];
            }
            for (int i = 0; i < valuesPositionTemplate1.Length; i++)
            {
                valuesPosition1[i] = valuesPositionTemplate1[i];
            }
        }

        /// <summary>
        /// modificare laborator 8 - ex 3 
        /// actualizare cu setLight1Values(); 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLight0Reset_Click(object sender, EventArgs e)
        {
            setLight0Values();
            setLight1Values();
            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();
            setLight1Default();
            setColorAmbientLigh1Default();
            setColorDifuseLigh1Default();
            setColorSpecularLigh1Default();

            GlControl1.Invalidate();
        }

        // control obiecte 3D 
        //Set?m variabila de stare pentru afi?area/scunderea sistemului de coordonate.
        private void setControlAxe(bool status)
        {
            if (status == false)
            {
                statusControlAxe = false;
                btnShowAxes.Text = "Axe Oxyz OFF";
            }
            else
            {
                statusControlAxe = true;
                btnShowAxes.Text = "Axe Oxyz ON";
            }
        }

        //Controlul axelor de coordonate (ON/OFF).
        private void btnShowAxes_Click(object sender, EventArgs e)
        {
            if (statusControlAxe == true)
            {
                setControlAxe(false);
            }
            else
            {
                setControlAxe(true);
            }
            GlControl1.Invalidate();
        }

        //Set?m variabila de stare pentru desenarea cubului. Valorile acceptabile sunt:
        //TRIANGLES = cubul este desenat, prin triunghiuri.
        //QUADS = cubul este desenat, prin quaduri.
        //OFF (sau orice altceva) = cubul nu este desenat.
        private void setCubeStatus(string status)
        {
            if (status.Trim().ToUpper().Equals("TRIANGLES"))
            {
                statusCube = "TRIANGLES";
            }
            else if (status.Trim().ToUpper().Equals("QUADS"))
            {
                statusCube = "QUADS";
            }
            else
            {
                statusCube = "OFF";
            }
        }
        private void btnCubeQ_Click(object sender, EventArgs e)
        {
            statusFiles = true;
            loadVertex();
            loadQList();
            setCubeStatus("QUADS");
            GlControl1.Invalidate();
        }
        private void btnCubeT_Click(object sender, EventArgs e)
        {
            statusFiles = true;
            loadVertex();
            loadTList();
            setCubeStatus("TRIANGLES");
            GlControl1.Invalidate();
        }
        private void btnResetObjects_Click(object sender, EventArgs e)
        {
            setCubeStatus("OFF");
            GlControl1.Invalidate();
        }

        /// <summary>
        /// modificare laborator 8 - ex 3 
        /// Sursa de lumin? va putea fi deplasat? folosind(a) 6 taste(3 axe spa?iale) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
            {
                valuesPosition1[0]++;
                GlControl1.Invalidate();
                Console.WriteLine("Sursa 1 de lumina. Incrementare valoare pe pozitia X. Valoarea este: " + valuesPosition1[0]);
            }

            if (e.KeyCode == Keys.A)
            {
                valuesPosition1[0]--;
                GlControl1.Invalidate();
                Console.WriteLine("Sursa 1 de lumina. Decrementare valoare pe pozitia X. Valoarea este: " + valuesPosition1[0]);
            }

            if (e.KeyCode == Keys.W)
            {
                valuesPosition1[1]++;
                GlControl1.Invalidate();
                Console.WriteLine("Sursa 1 de lumina. Incrementare valoare pe pozitia Y. Valoarea este: " + valuesPosition1[1]);
            }

            if (e.KeyCode == Keys.S)
            {
                valuesPosition1[1]--;
                GlControl1.Invalidate();
                Console.WriteLine("Sursa 1 de lumina. Decrementare valoare pe pozitia Y. Valoarea este: " + valuesPosition1[1]);
            }

            if (e.KeyCode == Keys.E)
            {
                valuesPosition1[2]++;
                GlControl1.Invalidate();
                Console.WriteLine("Sursa 1 de lumina. Incrementare valoare pe pozitia Z. Valoarea este: " + valuesPosition1[2]);
            }

            if (e.KeyCode == Keys.D)
            {
                valuesPosition1[2]--;
                GlControl1.Invalidate();
                Console.WriteLine("Sursa 1 de lumina. Decrementare valoare pe pozitia Z. Valoarea este: " + valuesPosition1[2]);
            }
        }

        // administrare mod 3D
        // (METODA PRINCIPAL?)
        private void GlControl1_Paint(object sender, PaintEventArgs e)
        {
            //Reseteaz? buffer-ele la valori default.
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            //Culoarea default a mediului.
            GL.ClearColor(Color.Black);

            //Set?ri preliminar? pentru mediul 3D.
            // declararea perspectivei spatiale 
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(camDepth, 4 / 3, 1, 10000);

            // declararea camerei (stare initiala) 
            Matrix4 lookat = Matrix4.LookAt(eyePosX, eyePosY, eyePosZ, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            // incarcarea modelului camerei 
            GL.LoadMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref lookat);

            // marimea suprafetei randate (scena 3D este proiectata pe aceasta) 
            GL.Viewport(0, 0, GlControl1.Width, GlControl1.Height);

            // corectii de adancime 
            GL.Enable(EnableCap.DepthTest);

            // corectii de adancime 
            GL.DepthFunc(DepthFunction.Less);

            // pornim iluminarea (daca avem permisiunea sa o facem) 
            if (lightON == true)
            {
                //Permite utilizarea ilumin?rii. Fara aceasta corec?ie iluminarea nu functioneaza.
                GL.Enable(EnableCap.Lighting);
            }
            else
            {
                //Dezactiveaz? utilizarea ilumin?rii.
                GL.Disable(EnableCap.Lighting);
            }

            //Se creeaza sursa de iluminare. In acest caz folosim o singura sursa.
            //Numarul de surse de lumini depinde de implementarea OpenGL, dar de obicei cel putin 8 surse sunt posibile simultan.
            GL.Light(LightName.Light0, LightParameter.Ambient, valuesAmbient0);
            GL.Light(LightName.Light0, LightParameter.Diffuse, valuesDiffuse0);
            GL.Light(LightName.Light0, LightParameter.Specular, valuesSpecular0);
            GL.Light(LightName.Light0, LightParameter.Position, valuesPosition0);

            // a doua sursa de iluminare 
            GL.Light(LightName.Light1, LightParameter.Ambient, valuesAmbient1);
            GL.Light(LightName.Light1, LightParameter.Diffuse, valuesDiffuse1);
            GL.Light(LightName.Light1, LightParameter.Specular, valuesSpecular1);
            GL.Light(LightName.Light1, LightParameter.Position, valuesPosition1);

            if ((lightON == true) && (lightON_0 == true))
            {
                //Activam sursa 0 de lumina. Fara aceasta actiune nu avem iluminare.
                GL.Enable(EnableCap.Light0);
            }
            else
            {
                //Dezactivam sursa 0 de lumina.
                GL.Disable(EnableCap.Light0);
            }

            if ((lightON == true) && (lightON_1 == true))
            {
                //Activam sursa 1 de lumina. Fara aceasta actiune nu avem iluminare.
                GL.Enable(EnableCap.Light1);
            }
            else
            {
                //Dezactivam sursa 1 de lumina.
                GL.Disable(EnableCap.Light1);
            }

            //Controlul rota?iei cu mouse-ul (2D).
            if (statusControlMouse2D == true)
            {
                GL.Rotate(mousePos.X, 0, 1, 0);
            }

            //Controlul rota?iei cu mouse-ul (3D).
            if (statusControlMouse3D == true)
            {
                GL.Rotate(mousePos.X, 0, 1, 1);
            }

            // modificare laborator 8 - ex 3 
            // Sursa de lumin? adi?ional? va putea fi deplasat? folosind: (b) mouse - ul (2 axe spa?iale).
            if (statusControlMouse2D == true)
            {
                valuesPosition1[0] = mousePos.X;
                // valuesPosition1[1] = mousePos.Y;
                // GlControl1.Invalidate();
            }

            // descrierea obiectelor 3D
            // Axe de coordonate.
            if (statusControlAxe == true)
            {
                axe.DeseneazaAxe();
            }

            //Desen?m obiectele 3D (cub format din quads sau triunghiuri).
            if (statusCube.ToUpper().Equals("QUADS"))
            {
                cub.DeseneazaCubQ(nQuadsList, arrQuadsList, arrVertex);
            }
            else if (statusCube.ToUpper().Equals("TRIANGLES"))
            {
                cub.DeseneazaCubT(nTrianglesList, arrTrianglesList, arrVertex);
            }

            //Limiteaz? viteza de randare pentru a nu supraîncarca unitatea GPU (în caz contrar randarea se face cât de rapid este posibil, pe 100% din resurse). 
            //Dezavantajul este c? o vitez? prea mic? poate afecta negativ cursivitatea anima?iei!
            //GraphicsContext.CurrentContext.SwapInterval = 1;                                         //Testati cu valori din 10 in 10!!!
            //GraphicsContext.CurrentContext.VSync = True

            GlControl1.SwapBuffers();
        }

    }
}