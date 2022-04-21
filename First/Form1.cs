using GMap.NET.WindowsForms;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace First
{
    public partial class Form : System.Windows.Forms.Form
    {
        private bool clicked = false;
        private readonly string connectionString = "Server=DESKTOP-N3K4LAO\\SSQE;Database=GeographicСoordinates;Trusted_Connection=True;";
        GMapOverlay markers = new GMapOverlay("markers");
        private GMapMarker marker;

        public Form()
        {
            InitializeComponent();
        }



        private void  gMapControl1_Load(object sender, EventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache; //выбор подгрузки карты – онлайн или из ресурсов
            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance; //какой провайдер карт используется (в нашем случае гугл) 
            gmap.MinZoom = 2; //минимальный зум
            gmap.MaxZoom = 16; //максимальный зум
            gmap.Zoom = 14; // какой используется зум при открытии
            gmap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter; // как приближает (просто в центр карты или по положению мыши)
            gmap.CanDragMap = true; // перетаскивание карты мышью
            gmap.DragButton = MouseButtons.Middle; // какой кнопкой осуществляется перетаскивание
            gmap.ShowCenter = false; //показывать или скрывать красный крестик в центре
            gmap.ShowTileGridLines = false; //показывать или скрывать тайлы
            gmap.Position = new GMap.NET.PointLatLng(55.59907310721567, 37.28551369055172);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM machine_coordinates", connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows) // если есть данные
                {
                    while (reader.Read()) // построчно считываем данные
                    {
                        double lat = reader.GetDouble(1);
                        double lng = reader.GetDouble(2);
                        string info = reader.GetString(3);


                        GMapMarker marker =
                            new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                            new GMap.NET.PointLatLng(lat, lng),
                                GMap.NET.WindowsForms.Markers.GMarkerGoogleType.blue_pushpin);
                        marker.ToolTipText = info;

                        markers.Markers.Add(marker);
                    }
                }
            }
            gmap.Overlays.Add(markers);
        }

        private void gMapControl1_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                marker = item;
                clicked = true;
            }
            else
            {
                marker = null;
            }
        }

        private void gmap_MouseClick(object sender, MouseEventArgs e)
        {
            if (clicked && marker != null && e.Button == MouseButtons.Right)
            {
                marker.Position = new GMap.NET.PointLatLng(
                gmap.FromLocalToLatLng(
                    e.Location.X, e.Location.Y).Lat,
                gmap.FromLocalToLatLng(
                    e.Location.X, e.Location.Y).Lng);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    int id = markers.Markers.TakeWhile(m => m != marker).Count() + 1;
                    connection.Open();
                    SqlCommand command = new SqlCommand(
                        $"UPDATE machine_coordinates " +
                        $"SET latitude ={marker.Position.Lat.ToString().Replace(",", ".")}, longitude ={marker.Position.Lng.ToString().Replace(",", ".")} " +
                        $"WHERE id = {id}", connection);
                    command.ExecuteNonQuery(); 
                }
            }
            clicked = false;
            marker = null;
        }
    }
}
