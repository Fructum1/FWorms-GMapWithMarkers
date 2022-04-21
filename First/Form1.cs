using GMap.NET.WindowsForms;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace First
{
    public partial class Form : System.Windows.Forms.Form
    {
        private bool _clicked = false;
        private readonly string _connectionString = "Server=DESKTOP-N3K4LAO\\SSQE;Database=GeographicСoordinates;Trusted_Connection=True;";
        GMapOverlay _markers = new GMapOverlay("markers");
        private GMapMarker _marker;


        public Form()
        {
            InitializeComponent();
        }

        private void  gMapControl1_Load(object sender, EventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            gmap.MinZoom = 2;
            gmap.MaxZoom = 16;
            gmap.Zoom = 14; 
            gmap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter; 
            gmap.CanDragMap = true; 
            gmap.DragButton = MouseButtons.Middle; 
            gmap.ShowCenter = false; 
            gmap.ShowTileGridLines = false; 
            gmap.Position = new GMap.NET.PointLatLng(55.59907310721567, 37.28551369055172);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM machine_coordinates", connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read()) 
                    {
                        double lat = reader.GetDouble(1);
                        double lng = reader.GetDouble(2);
                        string info = reader.GetString(3);

                        GMapMarker marker =
                            new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                            new GMap.NET.PointLatLng(lat, lng),
                                GMap.NET.WindowsForms.Markers.GMarkerGoogleType.blue_pushpin);

                        marker.ToolTipText = info;
                        _markers.Markers.Add(marker);
                    }
                }
            }
            gmap.Overlays.Add(_markers);
        }

        private void gMapControl1_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _marker = item;
                _clicked = true;
            }
            else
            {
                _marker = null;
            }
        }

        private void gmap_MouseClick(object sender, MouseEventArgs e)
        {
            if (_clicked && _marker != null && e.Button == MouseButtons.Right)
            {
                _marker.Position = new GMap.NET.PointLatLng(
                gmap.FromLocalToLatLng(
                    e.Location.X, e.Location.Y).Lat,
                gmap.FromLocalToLatLng(
                    e.Location.X, e.Location.Y).Lng);

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    int id = _markers.Markers.TakeWhile(m => m != _marker).Count() + 1;
                    connection.Open();
                    SqlCommand command = new SqlCommand(
                        $"UPDATE machine_coordinates " +
                        $"SET latitude ={_marker.Position.Lat.ToString().Replace(",", ".")}, longitude ={_marker.Position.Lng.ToString().Replace(",", ".")} " +
                        $"WHERE id = {id}", connection);
                    command.ExecuteNonQuery(); 
                }
            }

            _clicked = false;
            _marker = null;
        }
    }
}