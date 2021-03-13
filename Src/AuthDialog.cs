using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace BJ_Task.Src
{
    // Login dialog
    class AuthDialog
    {

        Activity activity;
        View dialog_view;
        AlertDialog dlg;
        AlertDialog.Builder builder;

        EditText etUsername;
        EditText etPassword;

        Button btnLogin;
        Action onHide;

        public AuthDialog(Activity activity)
        {
            this.activity = activity;
            dialog_view = activity.LayoutInflater.Inflate(Resource.Layout.AuthDialog, null);
            etUsername = dialog_view.FindViewById<EditText>(Resource.Id.etUsername);
            etPassword = dialog_view.FindViewById<EditText>(Resource.Id.etPassword);
            btnLogin = dialog_view.FindViewById<Button>(Resource.Id.btnLogin);

            btnLogin.Click += BtnLogin_Click;

            builder = new AlertDialog.Builder(activity);
            builder.SetView(dialog_view);
            dlg = builder.Create();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string token;
            // Simplest field modification.
            if (string.IsNullOrWhiteSpace(etUsername.Text))
            {
                Toast.MakeText(activity, "User name: This field is required", ToastLength.Long).Show();
                return;
            }
            if (string.IsNullOrWhiteSpace(etPassword.Text))
            {
                Toast.MakeText(activity, "Password: This field is required", ToastLength.Long).Show();
                return;
            }

            string result = ApiLayer.LogIn(etUsername.Text, etPassword.Text, out token);
            if (result != "ok")
            {// Display an authorization error message, but don't close the dialog.
                Toast.MakeText(activity, result, ToastLength.Long).Show();
                return;
            }

            Storage.Instance.SetTokenData(token);

            Hide();
            Toast.MakeText(activity, "Auth success", ToastLength.Long).Show();
        }

        // Such functions are usually done in the abstract parent class, but since it is single, then I see no reason to produce classes.
        public void OnHide(Action action)
        {
            onHide = action;
        }

        public void Show()
        {
            etPassword.Text = "";
            dlg.Show();
        }

        public void Hide()
        {
            dlg.Hide();
            if (onHide != null)
                onHide();
        }
    }
}