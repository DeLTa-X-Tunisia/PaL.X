using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PaL.X.Admin
{
    public static class BackendController
    {
        private static Process? _process;

        public static void Start()
        {
            if (_process != null && !_process.HasExited) return;

            // Chemin relatif depuis le dossier de sortie de l'Admin vers l'exécutable de l'API
            // Admin: ...\src\PaL.X.Admin\bin\Debug\net9.0-windows\
            // API:   ...\src\PaL.X.Api\bin\Debug\net9.0\PaL.X.Api.exe
            string apiPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\PaL.X.Api\bin\Debug\net9.0\PaL.X.Api.exe"));
            
            if (!File.Exists(apiPath))
            {
                MessageBox.Show($"Impossible de trouver l'exécutable API à : {apiPath}\nAssurez-vous d'avoir compilé le projet API.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try 
            {
                var psi = new ProcessStartInfo
                {
                    FileName = apiPath,
                    Arguments = "--urls \"http://localhost:5024\"", // Forcer le port 5024
                    UseShellExecute = false,
                    CreateNoWindow = true, // Masquer la console
                    WorkingDirectory = Path.GetDirectoryName(apiPath)
                };

                _process = Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur au démarrage du backend: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Stop()
        {
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                    _process.WaitForExit(2000);
                }
            }
            catch (Exception)
            {
                // Ignorer les erreurs d'arrêt : le processus sera forcé à se fermer
            }
            finally
            {
                if (_process != null)
                {
                    try
                    {
                        _process.Dispose();
                    }
                    catch
                    {
                        // Ignorer les erreurs de disposition
                    }

                    _process = null;
                }
            }
        }
    }
}
