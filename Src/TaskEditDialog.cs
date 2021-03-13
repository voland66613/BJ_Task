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

using BJ_Task.Src.DataStructures;



namespace BJ_Task.Src
{
    class TaskEditDialog
    {
        Activity ctx;
        View dialog_view;

        bool createNew;
        bool haveChanges;
        TaskData editableItem;

        AlertDialog dlg;
        AlertDialog.Builder builder;

        Action<bool> onHide;
        EditText etUsername;
        TextView tvUsername;
        EditText etEmail;
        TextView tvEmail;
        EditText etTaskText;
        CheckBox cbxCompleted;
        Button btnSave;

        public bool IsNewItem { get => createNew; }

        public TaskEditDialog(Activity activity)
        {
            ctx = activity;
            dialog_view = activity.LayoutInflater.Inflate(Resource.Layout.TaskEditDialog, null);

            etUsername = dialog_view.FindViewById<EditText>(Resource.Id.etUsername);
            tvUsername = dialog_view.FindViewById<TextView>(Resource.Id.tvUsername);
            etEmail = dialog_view.FindViewById<EditText>(Resource.Id.etEmail);
            tvEmail = dialog_view.FindViewById<TextView>(Resource.Id.tvEmail);
            etTaskText = dialog_view.FindViewById<EditText>(Resource.Id.etTaskText);
            cbxCompleted = dialog_view.FindViewById<CheckBox>(Resource.Id.cbxCompleted);
            btnSave = dialog_view.FindViewById<Button>(Resource.Id.btnSave);

            btnSave.Click += BtnSave_Click;

            builder = new AlertDialog.Builder(activity);
            builder.SetView(dialog_view);
            dlg = builder.Create();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (createNew)
            {
                // Simplest field modification.
                if (string.IsNullOrWhiteSpace(etUsername.Text))
                {
                    Toast.MakeText(ctx, "User name: This field is required", ToastLength.Long).Show();
                    return;
                }
                if (string.IsNullOrWhiteSpace(etEmail.Text))
                {
                    Toast.MakeText(ctx, "E-Mail: This field is required", ToastLength.Long).Show();
                    return;
                }
                else if (!etEmail.Text.Contains('@') || (etEmail.Text.IndexOf('.', etEmail.Text.IndexOf('@')) < 0)) //The simplest check, the point is that we check for the presence of the '@' character, and then periods after it (domain name)
                {
                    Toast.MakeText(ctx, "E-Mail: Please enter valid E-Mail address", ToastLength.Long).Show();
                    return;
                }
                if (string.IsNullOrWhiteSpace(etTaskText.Text))
                {
                    Toast.MakeText(ctx, "Task text: This field is required", ToastLength.Long).Show();
                    return;
                }

                var newItem = ApiLayer.CreateTask(etUsername.Text, etEmail.Text, etTaskText.Text);
                if (newItem.Id == -1) //Ошибка
                {
                    if (!string.IsNullOrEmpty(newItem.Username))
                        Toast.MakeText(ctx, "User name: " + newItem.Username, ToastLength.Long).Show();
                    if (!string.IsNullOrEmpty(newItem.Email))
                        Toast.MakeText(ctx, "E-Mail: " + newItem.Email, ToastLength.Long).Show();
                    if (!string.IsNullOrEmpty(newItem.Text))
                        Toast.MakeText(ctx, "Task text: " + newItem.Text, ToastLength.Long).Show();
                    return;
                }
                Toast.MakeText(ctx, "Task successfully added!", ToastLength.Long).Show();
                haveChanges = true;
                Hide();
            }
            else
            {
                haveChanges = false;
                if (editableItem.Text != etTaskText.Text)
                {
                    editableItem.Text = etTaskText.Text;
                    haveChanges = true;
                }
                if (cbxCompleted.Checked)
                {
                    if (editableItem.Status != 9)
                    {
                        if (haveChanges || editableItem.Status == 1) // If the text has been changed (including earlier), then we put a sign that the task is completed and edited
                            editableItem.Status = 9; //Completed / Admin edited
                        else
                            editableItem.Status = 10; //Completed
                        haveChanges = true;
                    }
                }
                else
                {
                    if (editableItem.Status != 1)
                    { // If the previous status was "Completed", then we also mark Admin edited.
                        editableItem.Status = 1; //New / Admin edited
                        haveChanges = true;
                    }
                }
                if (haveChanges)
                    ApiLayer.EditTask(Storage.Instance.GetToken(), editableItem.Id, editableItem.Text, editableItem.Status);
                Hide();
            }
        }

        public void SetEditItem(TaskData item)
        {
            haveChanges = false;
            if (item == null) //Creating a new ...
            {
                createNew = true;
                editableItem = new TaskData();
                cbxCompleted.Enabled = false;
                etUsername.Enabled = true;
                etEmail.Enabled = true;

                etUsername.Text = string.Empty;
                etEmail.Text = string.Empty;
                tvEmail.Text = string.Empty;
                cbxCompleted.Checked = false;
            }
            else //Editing...
            {
                createNew = false;
                editableItem = item;
                cbxCompleted.Enabled = true;
                etUsername.Enabled = false;
                etEmail.Enabled = false;
                etUsername.Text = editableItem.Username;
                etEmail.Text = editableItem.Email;
                etTaskText.Text = editableItem.Text;
                cbxCompleted.Checked = editableItem.Status >= 9;
            }

        }

        public void Show()
        {
            dlg.Show();
        }

        /// <summary>
        /// Sets the action when the dialog is hidden
        /// </summary>
        /// <param name = "action"> Action, as a bool parameter, value true indicates that changes have been made to the data. </param>
        public void OnHide(Action<bool> action)
        {
            onHide = action;
        }

        public void Hide()
        {
            dlg.Hide();
            if (onHide != null)
                onHide(haveChanges);
        }
    }
}