﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Capa_Controlador_AsistenciaYFaltas;
using System.IO;

namespace Modelo_Vista_AsistenciaYFaltas
{
    public partial class frm_importar_asistencia : Form
    {
        public frm_importar_asistencia()
        {
            InitializeComponent();
        }

        private Controlador controlador = new Controlador();


        private void frm_importar_asistencia_Load(object sender, EventArgs e)
        {
            dgvAsistencias.Rows.Clear();
        }


        private void CargarDatosEnGrid(string rutaArchivo)
        {
            var dt = new DataTable();
            dt.Columns.Add("Fecha", typeof(string));
            dt.Columns.Add("HoraEntrada", typeof(string));
            dt.Columns.Add("HoraSalida", typeof(string));
            dt.Columns.Add("IDEmpleado", typeof(string));

            string[] lineas;
            try
            {
                lineas = File.ReadAllLines(rutaArchivo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Se leyeron {lineas.Length} líneas del archivo.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);

            var lineasErroneas = new List<string>();

            foreach (var linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                int idx = linea.IndexOf("]:");
                if (idx < 0)
                {
                    lineasErroneas.Add(linea);
                    continue;
                }

                string fechaTexto = linea.Substring(1, idx - 1);
                string resto = linea.Substring(idx + 2);

                var partes = resto.Split(',');
                if (partes.Length != 2)
                {
                    lineasErroneas.Add(linea);
                    continue;
                }

                var horas = partes[0].Split('-');
                if (horas.Length != 2)
                {
                    lineasErroneas.Add(linea);
                    continue;
                }

                var idEmpleado = partes[1].TrimEnd('.');

                dt.Rows.Add(fechaTexto, horas[0], horas[1], idEmpleado);
            }

            dgvAsistencias.AutoGenerateColumns = true;
            dgvAsistencias.DataSource = dt;
            dgvAsistencias.AutoResizeColumns();

            if (lineasErroneas.Any())
            {
                MessageBox.Show($"Hubo {lineasErroneas.Count} línea(s) con formato inválido.",
                                "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }  
        }

        private void btn_examinar_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog
            {
                Filter = "Archivos de texto (*.txt)|*.txt",
                Title = "Seleccionar archivo de asistencia"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txt_RutaArchivo.Text = dlg.FileName;
                    CargarDatosEnGrid(dlg.FileName);
                }
            }
        }

        private void btn_importar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_RutaArchivo.Text))
            {
                MessageBox.Show("Seleccione un archivo antes de importar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                foreach (var linea in File.ReadAllLines(txt_RutaArchivo.Text).Where(l => !string.IsNullOrWhiteSpace(l)))
                {
                    controlador.InsertarAsistenciaPreliminar(linea);
                }

                using (var frmValida = new frm_validar_asistencia())
                {
                    frmValida.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar el archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvAsistencias_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
    }