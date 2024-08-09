using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.Design;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Accord.Video.FFMPEG;
using CefSharp;
using CefSharp.SchemeHandler;
using CefSharp.WinForms;
using GMap.NET.WindowsForms;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms.Markers;
using GMap.NET;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Collections.Specialized.BitVector32;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using CefSharp.DevTools.Page;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web.Compilation;
using static Emgu.CV.Fuzzy.FuzzyInvoke;

namespace TAISAT
{
    public partial class TAISAT : Form
    {
        string telemetry_CurrentDate;
        string telemetry_CurrentTime;
        //TO-DO 
        //power
        //TO-DO 
        List<string> logs = new List<string>();//CSV kaydı için oluşturulan liste 
        bool maximized = false;//Tek seferlik tam ekran moduna geçmek için gerekli değişken 
        //3D Simulation Değişkenleri
        string _3DSimExePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\3D SİM\TAISAT_3D_Sim.exe"; //TODO: BURADAKİ YOLU DÜZENLE PROJE DOSYASININ İÇİNE AL PROGRAMI
        [DllImport("user32.dll")] static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)] internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        Process simApplication;
        const int WS_BORDER = 8388608;
        const int WS_DLGFRAME = 4194304;
        const int WS_CAPTION = WS_BORDER | WS_DLGFRAME;
        const int WS_SYSMENU = 524288;
        const int WS_THICKFRAME = 262144;
        const int WS_MINIMIZE = 536870912;
        const int WS_MAXIMIZEBOX = 65536;
        const int GWL_STYLE = (int)-16L;
        const int GWL_EXSTYLE = (int)-20L;
        const int WS_EX_DLGMODALFRAME = (int)0x1L;
        const int SWP_NOMOVE = 0x2;
        const int SWP_NOSIZE = 0x1;
        const int SWP_FRAMECHANGED = 0x20;
        int count = 5;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetWindowLongA(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetWindowLongA(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        public void MakeExternalWindowBorderless(IntPtr MainWindowHandle)
        {
            int Style = 0;
            Style = GetWindowLongA(MainWindowHandle, GWL_STYLE);
            Style = Style & ~WS_CAPTION;
            Style = Style & ~WS_SYSMENU;
            Style = Style & ~WS_THICKFRAME;
            Style = Style & ~WS_MINIMIZE;
            Style = Style & ~WS_MAXIMIZEBOX;
            SetWindowLongA(MainWindowHandle, GWL_STYLE, Style);
            Style = GetWindowLongA(MainWindowHandle, GWL_EXSTYLE);
            SetWindowLongA(MainWindowHandle, GWL_EXSTYLE, Style | WS_EX_DLGMODALFRAME);
            SetWindowPos(MainWindowHandle, new IntPtr(0), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);
        }
        Label[] statusLabels;//0-7 Arası Uydu Statusu Belirten labellar  
        //Video Kaydı değişkenleri
        int width, height;
        int resolution = 480;
        string video_Record_Time = "00:00:00";
        FilterInfoCollection fCollection;
        VideoCaptureDevice video_Capture_Device;
        VideoFileWriter videoWriter = new VideoFileWriter();
        System.Drawing.Bitmap videoBitmap;
        TimeSpan currentTime;
        TimeSpan startTime;
        TimeSpan finishTime;
        TimeSpan elapsedTime;
        string[] cache; string log = ""; //Log   
        bool seperation = false; //Seperation
        //Port Okuma Değişkenleri
        SerialPort port;
        string buffer = string.Empty;
        int bufferPackageCount;
        int counter = 0;
        int bufferSize = 197;
        bool portIsOpen = false;
        string[] telemetryData;
        string[] partBuffer;
        //Port Okuma Değişkenleri 
        int _data = 0, data = 0, resetWait = 5; //İletişim kontrolü için değişkenler
        //Offline Map
        internal readonly GMapOverlay objects = new GMapOverlay("objects");
        GMapOverlay markers = new GMapOverlay("markers");
        GMapMarker sat;
        GMapMarker station = new GMarkerGoogle(
             new PointLatLng(38.398447983639656, 33.71120194610159),//Aksaray Hisar Atış Alanı Koordinatları
             GMarkerGoogleType.green_dot);
        GMapOverlay polyOverlay = new GMapOverlay("polygons");
        string mapCacheFolder = @"C:\Users\efeka\source\repos\TAISAT\Extensions\Map";
        //Offline Map
        //My Functions
        
        void InitGmap()//Gmap
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            gmapMap.MapProvider = GoogleSatelliteMapProvider.Instance;
            gmapMap.CacheLocation = mapCacheFolder;
            gmapMap.Position = new PointLatLng(station.Position.Lat, station.Position.Lng);//Aksaray Hisar Atış Alanı Koordinatları
            gmapMap.MinZoom = 3;
            gmapMap.MaxZoom = 20;
            gmapMap.Zoom = 14;
            gmapMap.Manager.CancelTileCaching();
            gmapMap.HoldInvalidation = false;
            markers.Markers.Add(station);
            gmapMap.Overlays.Add(markers);
            gmapMap.Dock = DockStyle.Fill;
        }
     
        void AddSatPointToGMAP(double lat, double lng)
        {
            sat = new GMarkerGoogle(
               new PointLatLng(lat, lng),
               GMarkerGoogleType.red_dot);
            sat.ToolTipMode = MarkerTooltipMode.Always;
            sat.ToolTipText = getDistance(new PointLatLng(sat.Position.Lat, sat.Position.Lng), new PointLatLng(station.Position.Lat, station.Position.Lng)).ToString("N2") + " m";
            markers.Markers.Add(sat);
            gmapMap.Overlays.Add(markers);
        }
        void AddPayloadPointToGMAP(double lat, double lng)
        {
            GMapMarker payload = new GMarkerGoogle(
                new PointLatLng(lat, lng),
                GMarkerGoogleType.blue_dot);

            payload.ToolTipMode = MarkerTooltipMode.Always;
            payload.ToolTipText = getDistance(new PointLatLng(payload.Position.Lat, payload.Position.Lng), new PointLatLng(sat.Position.Lat, sat.Position.Lng)).ToString("N2") + " m";
            markers.Markers.Add(payload);
            gmapMap.Overlays.Add(markers);
        }
        double _double(string x)
        {
            try
            {
                x = x.Replace(",", ".");
                return Convert.ToDouble(x, CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Invalid format for input: {x}. Exception: {ex.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting to double: {x}. Exception: {ex.Message}");
                return 0;
            }
        }
        void UpdateGMap(string lat1, string lng1, string lat2 = null, string lng2 = null)
        {
            polyOverlay.Polygons.Clear();
            markers.Markers.Clear();
            markers.Markers.Add(station);

            AddSatPointToGMAP(_double(lat1), _double(lng1));

            if (lat2 != null && lng2 != null)
            {
                AddPayloadPointToGMAP(_double(lat2), _double(lng2));
            }

            List<PointLatLng> points = new List<PointLatLng>
    {
        new PointLatLng(_double(lat1), _double(lng1)),
        new PointLatLng(station.Position.Lat, station.Position.Lng)
    };

            if (lat2 != null && lng2 != null)
            {
                points.Insert(1, new PointLatLng(_double(lat2), _double(lng2)));
            }

            GMapPolygon polygon = new GMapPolygon(points, "mypolygon")
            {
                Stroke = new Pen(Color.Red, 3),
                Fill = new SolidBrush(Color.Transparent)
            };
            polyOverlay.Polygons.Add(polygon);
            gmapMap.Overlays.Add(polyOverlay);
            gmapMap.Refresh();
        }

        double rad(double x)
        {
            return x * Math.PI / 180;
        }
        double getDistance(PointLatLng p1, PointLatLng p2)
        {
            var R = 6378137;
            var dLat = rad(p2.Lat - p1.Lat);
            var dLong = rad(p2.Lng - p1.Lng);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(rad(p1.Lat)) * Math.Cos(rad(p2.Lat)) *
              Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d;
        }
        void ToggleUI(bool toggle)
        {
            try
            {
                Type[] types = { typeof(TextBox), typeof(Button), typeof(ComboBox) };
                foreach (Type type in types)
                    foreach (var item in GetAll(this, type))
                        item.Enabled = toggle;
            }
            catch  {}
        }
        public IEnumerable<Control> GetAll(Control control, Type type)
        {
            try
            {
                var controls = control.Controls.Cast<Control>();
                return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                          .Concat(controls)
                                          .Where(c => c.GetType() == type);
            }
            catch (Exception) { MessageBox.Show("GetAll"); return null; }
        }
        public static string GetExternalDrivePath() //TODO: Bu fonksiyon çıkarılabilir diski otomatik bulur ve o diskin yolunu döndürür.
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in allDrives)
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    return drive.Name; 
                }
            }

            return null; 
        }
        void SaveFlightCSV()
        {
            try
            {
                var csv = new StringBuilder();
                csv.AppendLine("<PAKETNUMARASI>;<UYDUSTATUSU>;<HATAKODU>;<GONDERMESAATI>;<BASINC1>;<BASINC2>;<YUKSEKLIK1>;<YUKSEKLIK2>;<IRTIFAFARKI>;<INIŞHIZI>;<SICAKLIK>;<PILGERILIMI>;<GPS1LATITUDE>;<GPS1LONGITUDE>;<GPS1ALTITUDE>;<PITCH>;<ROLL>;<YAW>;<RHRH>;<IoTDATA>;<TAKIMNO>;");
                foreach (var log in logs)
                    csv.AppendLine(log);
                string filePath = Path.Combine(GetExternalDrivePath(), "telemetrydatas.csv");
                File.WriteAllText(filePath, csv.ToString());
                MessageBox.Show("CSV dosyası oluşturuldu" + filePath);
            }
            catch (Exception) { MessageBox.Show("SaveFlight"); }
        }
        void ListComPorts()
        {
            try
            {
                comboBox_COMPortTelemetry.Items.Clear();
                foreach (var port in SerialPort.GetPortNames())
                    comboBox_COMPortTelemetry.Items.Add(port);
            }
            catch (Exception) { MessageBox.Show("ListComPorts"); }
        } 
        private void AddTelemetryTable()
        {
            try
            {

                    if (dataGridView_telemetrytable.InvokeRequired)
                    {
                        dataGridView_telemetrytable.Invoke(new Action(() =>
                        {
                            dataGridView_telemetrytable.Rows.Add(cache);
                            dataGridView_telemetrytable.FirstDisplayedScrollingRowIndex = dataGridView_telemetrytable.RowCount - 1;
                        }));
                    }
                    else
                    {
                        dataGridView_telemetrytable.Rows.Add(cache);
                        dataGridView_telemetrytable.FirstDisplayedScrollingRowIndex = dataGridView_telemetrytable.RowCount - 1;
                    }

            }
            catch (Exception ex)
            {
                MessageBox.Show("AddTelemetryTable" + ex + "Veri uzunluğu sütun sayısıyla aynı olmayabilir!");
            }
        }
        void SaveFlightTxt()
        {
            try
            {
                string filePath = Path.Combine(GetExternalDrivePath(), "telemetrydatas.txt");
                File.AppendAllText(filePath, log);
            }
            catch { }
        }
        public void SerialPortProgram()
        {
            try
            {
                port.Open();
            }
            catch (Exception) { MessageBox.Show("SerialPortProgram"); }
        }
        public void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (!backgroundWorker1.IsBusy)
                    backgroundWorker1.RunWorkerAsync();
            }
            catch (Exception) { MessageBox.Show("Port_DataReceived"); }
        }
        void Key()
        {
            try
            {
                var Key = new char[] { '#', '#', '?', '?', '?', '?', '%' };
                port.Write(Key, 0, Key.Length);
            }
            catch
            {
            }
                    
            
        }
        void CameraCloseKey()
        {
            var cameraKey = new char[] { '#', '*', '?', '?', '?', '?', '#' };
            if (portIsOpen == true)
            {
                for (int i = 0; i < 5; i++)
                {
                    port.Write(cameraKey, 0, cameraKey.Length);
                    Thread.Sleep(50);
                }
            }
        }
        void CameraOpenKey()
        {
            var cameraKey = new char[] { '#', '*', '?', '?', '?', '?','#' };
            if (portIsOpen == true)
            {
                for (int i = 0; i < 5; i++)
                {
                    port.Write(cameraKey, 0, cameraKey.Length);
                    Thread.Sleep(50);
                }
            }
        }
        void RHRH_Mission()
        {
            try
            {
                char H1 = default(char);
                char H2 = default(char);
                string R1_String = R1_combo_box.SelectedItem.ToString();
                string R2_String = R2_combo_box.SelectedItem.ToString();
                char R1 = Convert.ToChar(R1_String); 
                char R2 = Convert.ToChar(R2_String);
                if (H1_combo_box.SelectedIndex == 0)
                {
                    H1 = 'R';
                    H1_combo_box.BackColor = Color.Red;
                }
                else if (H1_combo_box.SelectedIndex == 1)
                {
                    H1 = 'G';
                    H1_combo_box.BackColor = Color.Green;
                }
                else if (H1_combo_box.SelectedIndex == 2)
                {
                    H1 = 'B';
                    H1_combo_box.BackColor = Color.Blue;
                }
                if (H2_combo_box.SelectedIndex == 0)
                {
                    H2 = 'R';
                    H2_combo_box.BackColor = Color.Red;
                }
                else if (H2_combo_box.SelectedIndex == 1)
                {
                    H2 = 'G';
                    H2_combo_box.BackColor = Color.Green;
                }
                else if (H2_combo_box.SelectedIndex == 2)
                {
                    H2 = 'B';
                    H2_combo_box.BackColor = Color.Blue;
                }      
                if (H1 != default(char) && H2 != default(char))
                {
                    var data_RHRH = new char[] { '#','#',R1, H1, R2, H2, '#' };
                    if(portIsOpen==true)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            port.Write(data_RHRH, 0, data_RHRH.Length);
                            Thread.Sleep(50);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Lütfen önce seri port bağlantısını yapınız!!!",
                            "COM Port Bağlantısı Kurulmadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    
                }
            }
            catch (Exception) { MessageBox.Show("RHRH_Mission"); }
        }


        private bool IsSimulationRunning()
        {
            if (simApplication == null) return false;
            try
            {
                return !simApplication.HasExited;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void KillSimulations()
        {
            if (IsSimulationRunning())
            {
                try
                {
                    simApplication.Kill();
                    simApplication.WaitForExit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error killing the simulation: " + ex.Message);
                }
            }
        }
        public void StartSimulation()
        {
            KillSimulations();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = _3DSimExePath,
                Arguments = "-parentHWND " + gyro_3D_Panel.Handle.ToInt32() + " -popupwindow",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            simApplication = Process.Start(startInfo);
            simApplication.WaitForInputIdle();

            if (simApplication != null)
            {
                MoveWindow(simApplication.MainWindowHandle, 0, 0, gyro_3D_Panel.Width, gyro_3D_Panel.Height, true);
            }

            windowFixer.Start();
        }
        void Deploy()
        {
            try
            {
                    var data = new char[] { '*', '#','?','?','?','?','#' };
                    port.Write(data, 0, data.Length);
            }
            catch (Exception) { MessageBox.Show("Deploy"); }
        }
        void UpdateLabelText(Label label, string text) 
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new MethodInvoker(() => label.Text = text));
            }
            else
            {
                label.Text = text;
            }
        }
        void UpdateChart(Chart chart, string seriesName, string xValue, string yValue, bool isValueShownAsLabel)
        {
                chart.Series[seriesName].IsValueShownAsLabel = isValueShownAsLabel;
                chart.Series[seriesName].Points.AddXY(xValue, yValue);
                chart.Series[seriesName].Color = Color.Green;
                chart.Invalidate();
        }
        void SetChartAxisLabels(Chart chart, string xAxisTitle, string yAxisTitle)
        {
            chart.ChartAreas[0].AxisX.Title = xAxisTitle;
            chart.ChartAreas[0].AxisX.TitleForeColor = Color.Blue;
            chart.ChartAreas[0].AxisY.Title = yAxisTitle;
            chart.ChartAreas[0].AxisY.TitleForeColor = Color.Red;

        }

        void UpdateTextBoxLogs(string logs)
        {
            if (textBox_logs.InvokeRequired)
            {
                
                textBox_logs.Invoke(new Action<string>(UpdateTextBoxLogs), logs);
            }
            else
            {
                
                    textBox_logs.Text = logs;
            }
        }
        private void ProcessTelemetryData(string[] telemetryData)
        {
            try
            {
                string[] telemetryTableDatas =
                        {
                        Remove(telemetryData[0]),
                        Remove(telemetryData[1]),
                        Remove(telemetryData[2]),
                        (Remove(telemetryData[3])+" "+Remove(telemetryData[4])),
                        Remove(telemetryData[5]),
                        Remove(telemetryData[6]),
                        Remove(telemetryData[7]),
                        Remove(telemetryData[8]),
                        Remove(telemetryData[9]),
                        Remove(telemetryData[10]),
                        Remove(telemetryData[11]),
                        Remove(telemetryData[12]),
                        Remove(telemetryData[13]),
                        Remove(telemetryData[14]),
                        Remove(telemetryData[15]),
                        Remove(telemetryData[16]),
                        Remove(telemetryData[17]),
                        Remove(telemetryData[18]),
                        Remove(telemetryData[19]),
                        Remove(telemetryData[20]),
                        Remove(telemetryData[21])
                        };
                cache = telemetryTableDatas;
                Task TelemetryTable = Task.Run(() => AddTelemetryTable());
                Task Charts = Task.Run(() => UpdateCharts());
                Task Labels = Task.Run(() => UpdateUI());
                string telemetry = FormatTelemetryData(cache);
                logs.Add(telemetry);
                SaveTelemetryData(telemetryData);
                UpdateStatusLabels(telemetryData);  
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in ProcessTelemetryData: " + ex.Message);
            }
        }

        private string FormatTelemetryData(string[] cache)
        {
            return string.Join(";", cache);
        }
        string Remove(string Index)
        {
            Index = Index.Replace("<", "").Replace(">", "");
            return Index;
        }
        string LocationDataSeperator(string data)
        {

                int Index = data.IndexOf(',');
                data = data.Remove(Index, data.Length - Index);
                string hour = data.Substring(0, Index - 2);
                string minute = data.Substring(Index - 2);

                return hour + "," + minute; 
        }
        private void UpdateUI()
        {
            try
            {
                UpdateLabelText(label_Package_No, Remove(telemetryData[0]));
                UpdateLabelText(label_status, Remove(telemetryData[1]));
                UpdateLabelText(label_Error_Code, Remove(telemetryData[2]));
                telemetry_CurrentDate = telemetryData[3].Replace("<", "");
                telemetry_CurrentTime = telemetryData[4].Replace(">", "");
                UpdateLabelText(label_containerPressure, Remove(telemetryData[6]));
                UpdateLabelText(label_payloadPressure, Remove(telemetryData[5]));
                UpdateLabelText(label_containerAltitude, Remove(telemetryData[8]));
                UpdateLabelText(label_payloadAltitude, Remove(telemetryData[7]));
                UpdateLabelText(label_AltitudeDiff, Remove(telemetryData[9]));
                UpdateLabelText(label_payloadVelocity, Remove(telemetryData[10]));
                UpdateLabelText(label_payloadTemperature, Remove(telemetryData[11]));
                UpdateLabelText(label_payloadBataryVoltage, Remove(telemetryData[12]));
                UpdateLabelText(label_payloadGPSLatitude, Remove(telemetryData[13]));
                UpdateLabelText(label_payloadGPSLongitude, Remove(telemetryData[14]));
                UpdateLabelText(label_payloadGPSAltitude, Remove(telemetryData[15]));
                UpdateLabelText(label_payloadPitch, Remove(telemetryData[16]));
                UpdateLabelText(label_payloadRoll, Remove(telemetryData[17]));
                UpdateLabelText(label_payloadYaw, Remove(telemetryData[18]));
                UpdateLabelText(label_IoT_Data, Remove(telemetryData[20]));
                UpdateLabelText(label_carrierGPSLatitude, Remove(telemetryData[22]));
                UpdateLabelText(label_carrierGPSLongitude, Remove(telemetryData[23]));
                UpdateLabelText(label_Team_ID, Remove(telemetryData[21]));
                UpdateLabelText(label_carrierTemperature,Remove(telemetryData[24]));
                UpdateLabelText(label_carrierVoltage, Remove(telemetryData[25]));
            }
            catch { }
        }

        private void UpdateCharts()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke((Action)UpdateCharts);
                    return;
                }

                SetChartAxisLabels(payloadGPSAltitude_Chart, "Current Time", "GPS Altitude(m)");
                SetChartAxisLabels(temperature_Chart, "Current Time", "Temperature(°C)");
                SetChartAxisLabels(batterVoltage_Chart, "Current Time", "Battery Voltage(V)");
                SetChartAxisLabels(velocity_Chart, "Current Time", "Velocity(m/s)");
                SetChartAxisLabels(differenceAltitude_Chart, "Current Time", "Altitude Difference(m)");
                SetChartAxisLabels(payloadAltitude_Chart, "Current Time", "Payload Altitude(m)");
                SetChartAxisLabels(carrierAltitude_Chart, "Current Time", "Carrier Altitude(m)");
                SetChartAxisLabels(payloadPressure_Chart, "Current Time", "Payload Pressure(Pa)");
                SetChartAxisLabels(carrierPressure_Chart, "Current Time", "Carrier Pressure(Pa)");
                SetChartAxisLabels(IoT_Data_Chart, "Current Time", "IoT Temperature(°C)");

                string telemetry_CurrentTime = telemetryData[4].Replace("/", ":").Replace(">", "");

                UpdateChart(payloadGPSAltitude_Chart, "P_GPSAltitude", telemetry_CurrentTime, Remove(telemetryData[15]), true);
                UpdateChart(temperature_Chart, "Temperature", telemetry_CurrentTime, Remove(telemetryData[11]), true);
                UpdateChart(batterVoltage_Chart, "B_Voltage", telemetry_CurrentTime, Remove(telemetryData[12]), true);
                UpdateChart(velocity_Chart, "Velocity", telemetry_CurrentTime, Remove(telemetryData[10]), true);
                UpdateChart(differenceAltitude_Chart, "D_Altitude", telemetry_CurrentTime, Remove(telemetryData[9]), true);
                UpdateChart(payloadAltitude_Chart, "P_Altitude", telemetry_CurrentTime, Remove(telemetryData[7]), true);
                UpdateChart(carrierAltitude_Chart, "C_Altitude", telemetry_CurrentTime, Remove(telemetryData[8]), true);
                UpdateChart(payloadPressure_Chart, "P_Pressure", telemetry_CurrentTime, Remove(telemetryData[5]), true);
                UpdateChart(carrierPressure_Chart, "C_Pressure", telemetry_CurrentTime, Remove(telemetryData[6]), true);
                UpdateChart(IoT_Data_Chart, "Temperature", telemetry_CurrentTime, Remove(telemetryData[20]), true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // Hata mesajını göster
            }
        }


        private void UpdateStatusLabels(string[] telemetryData)
        {
            int status = Int16.Parse(Remove(telemetryData[1]));
            for (int i = 0; i < statusLabels.Length; i++)
            {
                statusLabels[i].BackColor = (i == status) ? Color.Lime : Color.Red;
                if (i == 5 && i == status)
                {
                    label_status05_devamı.BackColor = Color.Lime;
                }
                else
                {
                    label_status05_devamı.BackColor = Color.Red;
                }
            }

            if (status == 3 || status == 8)
            {
                if (!seperation)
                {
                    seperation = true;
                    try
                    {
                        new UdpClient().Send(Encoding.ASCII.GetBytes("separation"), Encoding.ASCII.GetBytes("separation").Length, "127.0.0.1", 50000);
                    }
                    catch { }
                }
            }
        }

        
        void UpdateSerialMonitorListbox(string partBuffer)
        {
            if (serialMonitorListBox.InvokeRequired)
            {
                serialMonitorListBox.Invoke(new Action<string>(UpdateSerialMonitorListbox), partBuffer);
            }
            else
            {
                serialMonitorListBox.Items.Add(partBuffer);
                serialMonitorListBox.TopIndex = serialMonitorListBox.Items.Count - 1;
            }
        }
        void AddToSerialMonitorListBox()
        {
            UpdateSerialMonitorListbox(partBuffer[counter]);
        }
        private void SaveTelemetryData(string[] telemetryData)
        {
            log =
                "---------------------------------------------------" + Environment.NewLine +
                telemetryData[0] + " numaralı " + telemetry_CurrentTime + "--" + DateTime.Now.ToString("HH:mm:ss") + " saatinde gelen veriler" + Environment.NewLine +
                "PAKET NUMARASI:" + telemetryData[0] + Environment.NewLine +
                "UYDU STATÜSÜ:" + telemetryData[1] + Environment.NewLine +
                "HATA KODU:" + telemetryData[2] + Environment.NewLine +
                "GÖNDERME SAATİ:" + telemetry_CurrentDate + " - " + telemetry_CurrentTime + Environment.NewLine +
                "BASINÇ 1:" + telemetryData[6] + Environment.NewLine +
                "BASINÇ 2:" + telemetryData[5] + Environment.NewLine +
                "YÜKSEKLİK1:" + telemetryData[8] + Environment.NewLine +
                "YÜKSEKLİK2:" + telemetryData[7] + Environment.NewLine +
                "İRTİFA FARKI:" + telemetryData[9] + Environment.NewLine +
                "İNİŞ HIZI:" + telemetryData[10] + Environment.NewLine +
                "SICAKLIK:" + telemetryData[11] + Environment.NewLine +
                "PİL GERİLİMİ:" + telemetryData[12] + Environment.NewLine +
                "GPS LATİTUDE:" + telemetryData[13] + Environment.NewLine +
                "GPS LONGİTUDE:" + telemetryData[14] + Environment.NewLine +
                "GPS ALTİTUDE:" + telemetryData[15] + Environment.NewLine +
                "PİTCH:" + telemetryData[16] + Environment.NewLine +
                "ROLL:" + telemetryData[17] + Environment.NewLine +
                "YAW:" + telemetryData[18] + Environment.NewLine +
                "RHRH:" + telemetryData[21] + Environment.NewLine +
                "IoT DATA:" + telemetryData[20] + Environment.NewLine +
                "TAKIM NO:" + telemetryData[21] + Environment.NewLine +
                "---------------------------------------------------" + Environment.NewLine;
                Task saveTXTFile = Task.Run(() => SaveFlightTxt());
        }

        

        public TAISAT()
        {
            InitializeComponent();    
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (start_Record_Button.Text == "Stop Record")
                {
                    videoBitmap = (System.Drawing.Bitmap)eventArgs.Frame.Clone();
                    videoWriter.WriteVideoFrame(videoBitmap);
                    camera_Picture_Box.Image = videoBitmap;
                }
                else
                {
                    videoBitmap = (System.Drawing.Bitmap)eventArgs.Frame.Clone();
                    camera_Picture_Box.Image = videoBitmap;
                }
            }
            catch  {  }
        }

        private void TAISAT_Load(object sender, EventArgs e)
        {
            try
            {
                if (!maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.WindowState = FormWindowState.Maximized;
                    this.FormBorderStyle = FormBorderStyle.None;
                    MinimumSize = this.Size;
                    MaximumSize = this.Size;
                    maximized = true;
                }
            }
            catch (Exception) { MessageBox.Show("Windows maximization"); }

            try { Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Charts"); }
            catch (Exception) { }
            try
            {
                InitGmap();
                dataGridView_telemetrytable.Columns.Add("PAKET NUMARASI", "PAKET NUMARASI");
                dataGridView_telemetrytable.Columns.Add("UYDU STATÜSÜ", "UYDU STATÜSÜ");
                dataGridView_telemetrytable.Columns.Add("HATA KODU", "HATA KODU");
                dataGridView_telemetrytable.Columns.Add("GÖNDERME SAATİ", "GÖNDERME SAATİ");
                dataGridView_telemetrytable.Columns.Add("BASINÇ1", "BASINÇ1");
                dataGridView_telemetrytable.Columns.Add("BASINÇ2", "BASINÇ2");
                dataGridView_telemetrytable.Columns.Add("YÜKSEKLİK1", "YÜKSEKLİK1");
                dataGridView_telemetrytable.Columns.Add("YÜKSEKLİK2", "YÜKSEKLİK2");
                dataGridView_telemetrytable.Columns.Add("İRTİFA FARKI", "İRTİFA FARKI");
                dataGridView_telemetrytable.Columns.Add("İNİŞ HIZI", "İNİŞ HIZI");
                dataGridView_telemetrytable.Columns.Add("SICAKLIK", "SICAKLIK");
                dataGridView_telemetrytable.Columns.Add("PİL GERİLİMİ", "PİL GERİLİMİ");
                dataGridView_telemetrytable.Columns.Add("GPS1 LATITUDE", "GPS1 LATITUDE");
                dataGridView_telemetrytable.Columns.Add("GPS1 LONGITUDE", "GPS1 LONGITUDE");
                dataGridView_telemetrytable.Columns.Add("GPS1 ALTITUDE", "GPS1 ALTITUDE");
                dataGridView_telemetrytable.Columns.Add("PITCH", "PITCH");
                dataGridView_telemetrytable.Columns.Add("ROLL", "ROLL");
                dataGridView_telemetrytable.Columns.Add("YAW", "YAW");
                dataGridView_telemetrytable.Columns.Add("RHRH", "RHRH");
                dataGridView_telemetrytable.Columns.Add("IoT DATA", "IoT DATA");
                dataGridView_telemetrytable.Columns.Add("TAKIM NO", "TAKIM NO");
                dataGridView_telemetrytable.Columns[3].Width = 100;   
                dataGridView_telemetrytable.Columns[6].Width = 65;
                dataGridView_telemetrytable.Columns[7].Width = 65;
                dataGridView_telemetrytable.Columns[10].Width = 62;
                dataGridView_telemetrytable.Columns[11].Width = 62;
                dataGridView_telemetrytable.Columns[12].Width = 62;
                dataGridView_telemetrytable.Columns[13].Width = 62;
                dataGridView_telemetrytable.Columns[14].Width = 62;
                if (resolution == 480) { width = 640; height = 480; }
                if (resolution == 720) { width = 1280; height = 720; }
                if (resolution == 1080) { width = 1920; height = 1080; }
                statusLabels = new Label[6] { label_status00, label_status01, label_status02, label_status03, label_status04, label_status05 };
                fCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                foreach (FilterInfo item in fCollection)
                {
                    combo_Box_Camera.Items.Add(item.Name);
                    video_Capture_Device = new VideoCaptureDevice();
                }
                ListComPorts();
            }
            catch (Exception ex) { MessageBox.Show("Form1_Load" + ex.Message); }
            this.WindowState = FormWindowState.Maximized;
            this.MinimizeBox = false;
            
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label_payloadAltitude_Click(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void payloadPressure_Chart_Load(object sender, EventArgs e)
        {

        }

        private void carrierPressure_Chart_Load_1(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel7_Paint(object sender, PaintEventArgs e)
        {

        }



        private void TAISAT_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                KillSimulations();
                if (video_Capture_Device.IsRunning)
                { video_Capture_Device.Stop(); videoWriter.Close(); }
            }
            catch (Exception) { MessageBox.Show("Form1_FormClosing"); }
        }
        void comboBox_COMPortTelemetry_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                port = new SerialPort(comboBox_COMPortTelemetry.SelectedItem.ToString());
                port.BaudRate= Int32.Parse("115200");//TODO: Burayı düzelt direkt sayı koyma baudrate için combobox'ı kullan.
                port.Parity= Parity.None;
                port.DataBits = 8;
                port.StopBits=StopBits.One;
                port.DataReceived += Port_DataReceived;
            }
            catch (Exception) { MessageBox.Show("comboBox_COMPortTelemetry_SelectedIndexChanged"); }
        }

        private void start_Record_Button_Click(object sender, EventArgs e)
        {
            try
            {
                if (video_Capture_Device.IsRunning == true && start_Record_Button.Text == "Start Record")
                {
                    var form2 = new Form2
                    {
                        ShowInTaskbar = false,
                        MinimizeBox = false,
                        MaximizeBox = false,
                    };
                    form2.StartPosition = FormStartPosition.CenterParent;
                    form2.ShowDialog(this);
                    if (Form2.videorecordpath != null)
                    {
                        string filePath = "" + Form2.videorecordpath + "\\" + "videoTaisat_" + DateTime.Now.ToString().Replace(':', '.').Replace(' ', '_') + ".avi";
                        videoWriter.Open(@filePath, width, height, 30, VideoCodec.MPEG4, 5000000);
                        startTime = DateTime.Now.TimeOfDay;
                        timer_videoRecordTime.Start();
                        start_Record_Button.Text = "Stop Record";

                    }
                    else
                    {
                        MessageBox.Show("Lütfen videonun kaydedileceği yolu seçiniz !!!", "Kayıt Yolu Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    }
                }
                else if (start_Record_Button.Text == "Stop Record")
                {
                    videoWriter.Close();
                    finishTime = DateTime.Now.TimeOfDay;
                    timer_videoRecordTime.Stop();
                    start_Record_Button.Text = "Start Record";
                    video_Record_Time = "00.00.00";
                }
                else
                {
                    MessageBox.Show("Lütfen önce kameraya bağlanın !!!", "Kamera Bağlantısı Yapılmadı", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("start_Record_Button_Click" + ex.Message);
            }

        }
        #region picturebox_uzerine_metin_yazma
        private void camera_Picture_Box_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Rockwell", 9, FontStyle.Bold))
            {
                e.Graphics.DrawString(telemetry_CurrentDate + " " + telemetry_CurrentTime, myFont, Brushes.Black, new System.Drawing.Point(2, 2));
                e.Graphics.DrawString(video_Record_Time, myFont, Brushes.Red, new System.Drawing.Point(214, 167));
            }
        }
        #endregion
        private void timer_videoRecordTime_Tick(object sender, EventArgs e)
        {
            try
            {
                currentTime = DateTime.Now.TimeOfDay;
                elapsedTime = currentTime.Subtract(startTime);
                video_Record_Time = elapsedTime.ToString(@"hh\:mm\:ss");
            }
            catch {}
        }

        private void status_label_Click(object sender, EventArgs e)
        {

        }

        private void gmapRefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialMonitorListBox.Items.Count != 0)
                {
                    bool isPayloadLongitudeValid = !string.IsNullOrWhiteSpace(label_payloadGPSLongitude.Text) && label_payloadGPSLongitude.Text != "0";
                    bool isPayloadLatitudeValid = !string.IsNullOrWhiteSpace(label_payloadGPSLatitude.Text) && label_payloadGPSLatitude.Text != "0";
                    bool isCarrierLongitudeValid = !string.IsNullOrWhiteSpace(label_carrierGPSLongitude.Text) && label_carrierGPSLongitude.Text != "0";
                    bool isCarrierLatitudeValid = !string.IsNullOrWhiteSpace(label_carrierGPSLatitude.Text) && label_carrierGPSLatitude.Text != "0";
                    if (isPayloadLongitudeValid && isPayloadLatitudeValid && isCarrierLongitudeValid && isCarrierLatitudeValid)
                    {
                        UpdateGMap(
                            LocationDataSeperator(label_carrierGPSLatitude.Text.Replace(".", ",")),
                            LocationDataSeperator(label_carrierGPSLongitude.Text.Replace(".", ",")),
                            LocationDataSeperator(label_payloadGPSLatitude.Text.Replace(".", ",")),
                            LocationDataSeperator(label_payloadGPSLongitude.Text.Replace(".", ","))
                        );
                    }
                    else if (isPayloadLatitudeValid && isPayloadLongitudeValid)
                    {
                            UpdateGMap(
                            LocationDataSeperator(label_payloadGPSLatitude.Text.Replace(".", ",")),
                            LocationDataSeperator(label_payloadGPSLongitude.Text.Replace(".", ","))
                        );
                    }
                }
            }
            catch { }
            
        }


        private void windowFixer_Tick(object sender, EventArgs e)
        {
            try
            {
                MoveWindow(simApplication.MainWindowHandle, 0, 0, gyro_3D_Panel.Width, gyro_3D_Panel.Height, true);
                SetParent(simApplication.MainWindowHandle, gyro_3D_Panel.Handle);
                MakeExternalWindowBorderless(simApplication.MainWindowHandle);
                count--;
                if (count == 0)
                { windowFixer.Stop(); ToggleUI(true); }
                else
                    ToggleUI(false);
            }
            catch { }
        }



        private void save_Data_Click(object sender, EventArgs e)
        {
            try
            {
                Task saveCSVFile = Task.Run (() =>SaveFlightCSV());
            }
            catch (Exception)
            {
                MessageBox.Show("save_Data_Click");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (button_telemetryCOMPortOpenClose.BackColor == Color.Lime) ListComPorts();
                else
                {
                    port.Close();
                    port.Open();
                }
            }
            catch (Exception) { MessageBox.Show("button_refreshCOMPort_Click"); }
        }


        private void button_Manuel_Deploy_Click(object sender, EventArgs e)
        {
            try
            {
                for(int i=0; i==5; i++) 
                {
                    Task TaskDeploy = Task.Run(() => Deploy());
                }      
            }
            catch (Exception) { MessageBox.Show("button_MANUAL_DEPLOY_Click"); }
        }

       

        private void Send_RHRH_button_Click(object sender, EventArgs e)
        {
            RHRH_Mission();
        }

        private void button_Form_Close_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataKeySender.Enabled) { dataKeySender.Stop(); } 
                Close();
                KillSimulations();
            }
            catch 
            {
            }
            
        }

        private void TAISAT_MouseEnter_1(object sender, EventArgs e)
        {

        }

        private void TAISAT_MaximizedBoundsChanged(object sender, EventArgs e)
        {

        }

        private void TAISAT_LocationChanged(object sender, EventArgs e)
        {

        }

        private void label_status05_Click(object sender, EventArgs e)
        {

        }

        private void button_telemetryCOMPortOpenClose_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (button_telemetryCOMPortOpenClose.BackColor != Color.Red)
                {
                    if (comboBox_COMPortTelemetry.SelectedIndex != -1)
                    {
                        SerialPortProgram();
                        gmapRefreshTimer.Start();
                        button_telemetryCOMPortOpenClose.Text = "Close COM Port";
                        button_telemetryCOMPortOpenClose.BackColor = Color.Red;
                        MessageBox.Show("Seri port bağlantısı başarıyla sağlandı!"
                            , "Seri Port Bağlantı Bildirimi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        portIsOpen = true;
                        if (!dataKeySender.Enabled) dataKeySender.Start();
                        StartSimulation();
                    }
                    else { MessageBox.Show("COM Seçimi yapınız."); }
                }
                else
                {
                    if (dataKeySender.Enabled) dataKeySender.Stop();
                    gmapRefreshTimer.Stop();
                    Task saveCSVFile = Task.Run(() => SaveFlightCSV());
                    serialMonitorListBox.Items.Clear();
                    port.Close();
                    portIsOpen = false;
                    button_telemetryCOMPortOpenClose.Text = "Open COM Port";
                    button_telemetryCOMPortOpenClose.BackColor = Color.Lime;
                    MessageBox.Show("Uydu Görevi Tamamlandı!", "TAISAT Uydu Görev Bilgisi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    KillSimulations();
                }
            }
            catch (Exception) { MessageBox.Show("button_telemetryCOMPortOpenClose_Click"); }
        }

        private void button_refresh_COM_Port_Click(object sender, EventArgs e)
        {
            try
            {
                if (button_telemetryCOMPortOpenClose.BackColor == Color.Lime) ListComPorts();
                else
                {
                    port.Close();
                    port.Open();
                }
            }
            catch (Exception) { MessageBox.Show("button_refreshCOMPort_Click"); }
        }

        private void button_Map_Switch_Click(object sender, EventArgs e)
        {

        }
        private void dataKeySender_Tick(object sender, EventArgs e)
        {
            try
            {
                    Key();
            }
            catch { }
          
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        void ReadPort()
        {
            buffer += port.ReadExisting();
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Task PortProcces = Task.Run(() => ReadPort());
                bufferPackageCount = buffer.Split('\n').Length;
                partBuffer = buffer.Split('\n');
                if (counter != bufferPackageCount && partBuffer[counter].Split(',').Length == 27 )
                {                  
                        telemetryData = partBuffer[counter].Split(',');
                        Task task4 = Task.Run(() => AddToSerialMonitorListBox());
                        ProcessTelemetryData(telemetryData);
                        string gyroData = label_payloadPitch.Text + "," + label_payloadYaw.Text + "," + label_payloadRoll.Text;
                        try { new UdpClient().Send(Encoding.ASCII.GetBytes(gyroData), Encoding.ASCII.GetBytes(gyroData).Length, "127.0.0.1", 11000); } catch (Exception ex ){ MessageBox.Show("Simülasyon hatası" + ex.Message); }
                        counter++;
                }
                else
                {
                    UpdateTextBoxLogs(partBuffer[counter]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in backgroundWorker1_DoWork: " + ex.Message);
            }
        }
        private void open_Close_Video_Button_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (video_Capture_Device.IsRunning == true)
                {
                    CameraCloseKey();
                    video_Capture_Device.Stop();
                    open_Close_Video_Button.Text = "Open Camera";
                }
                else if (video_Capture_Device.IsRunning == false && combo_Box_Camera.SelectedIndex != -1)
                {
                    CameraOpenKey();
                    video_Capture_Device = new VideoCaptureDevice(fCollection[combo_Box_Camera.SelectedIndex].MonikerString);
                    video_Capture_Device.NewFrame += VideoCaptureDevice_NewFrame;
                    video_Capture_Device.Start();
                    open_Close_Video_Button.Text = "Close Camera";
                }
                else
                {
                    MessageBox.Show("Lütfen kamera seçimi yapınız!", "Kamera Seçimi Yapılmadı", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
            }
            catch (Exception)
            { MessageBox.Show("open_Close_Video_Button_Click"); }
        }
        
       
    }
}

