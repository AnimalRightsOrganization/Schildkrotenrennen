using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

namespace NetCoreServer.Utils
{
    public class UpdateManager
    {
        void Start()
        {
            LoginController login = new LoginController();
            UserController user = new UserController();
            UpdateController update = new UpdateController(login, user);
            update.Update();
        }
    }

    public delegate void LoginEventDelegate();
    public class LoginController
    {
        public LoginEventDelegate LoginEvent;
        public void Trigger()
        {
            LoginEvent.Invoke();
        }
    }
    public class UserController
    {
        int count;
        public void UpdateUsersOnMap()
        {
            count++;
            Debug.Print($"UpdateUsersOnMap..{count}");
        }
    }
    public class UpdateController
    {
        private UserController _userController;
        private BackgroundWorker _backgroundWorker;

        public UpdateController(LoginController loginController, UserController userController)
        {
            _userController = userController;
            loginController.LoginEvent += Update;
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
            _backgroundWorker.WorkerReportsProgress = true;
        }

        public void Update()
        {
            _backgroundWorker.RunWorkerAsync();
        }

        public void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(66);
                // Do the long-duration work here, and optionally
                // send the update back to the UI thread...
                int p = 0;// set your progress if appropriate
                object param = "something"; // use this to pass any additional parameter back to the UI
                _backgroundWorker.ReportProgress(p, param);
            }
        }

        // This event handler updates the UI
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Update the UI here
            _userController.UpdateUsersOnMap();
        }
    }
}