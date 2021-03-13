using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using BJ_Task.Src;
using System.Collections.Generic;
using BJ_Task.Src.DataStructures;

namespace BJ_Task
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        TaskListAdapter tasklist;
        int page = 1;
        int total_items = 0;
        string orderBy = "id";
        bool orderDesc = false;
        AuthDialog authDialog;
        TaskEditDialog taskEditDialog;


        // To manage the Visible states of login / logout items
        IMenuItem mnuAuth;
        IMenuItem mnuLogoff;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            //Создаем адаптер для отображения
            var lvTasks = FindViewById<ListView>(Resource.Id.lvTasks);
            tasklist = new TaskListAdapter(this, UpdateTasks(true));
            lvTasks.Adapter = tasklist;

            // Initialize the "storage"
            Storage.Instance.Init();


            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
            FloatingActionButton fabNext = FindViewById<FloatingActionButton>(Resource.Id.fabNextPage);
            FloatingActionButton fabPrev = FindViewById<FloatingActionButton>(Resource.Id.fabPrevPage);
            fabNext.Click += FabNext_Click;
            fabPrev.Click += FabPrev_Click;
            fabPrev.Visibility = ViewStates.Invisible;

            RadioButton rb = FindViewById<RadioButton>(Resource.Id.rbSortId);
            rb.Click += Rb_Click;
            rb = FindViewById<RadioButton>(Resource.Id.rbSortUsename);
            rb.Click += Rb_Click;
            rb = FindViewById<RadioButton>(Resource.Id.rbSortEmail);
            rb.Click += Rb_Click;
            rb = FindViewById<RadioButton>(Resource.Id.rbSortStatus);
            rb.Click += Rb_Click;

            lvTasks.ItemClick += LvTasks_Click;

            // Instance dialogs and set actions when hiding dialogs
            authDialog = new AuthDialog(this);
            authDialog.OnHide(() => { // When hiding the authorization dialog, we check whether it has been completed. And if so, change the menu items
                if (!string.IsNullOrEmpty(Storage.Instance.GetToken()))
                {
                    mnuAuth.SetVisible(false);
                    mnuLogoff.SetVisible(true);
                }
            });
            taskEditDialog = new TaskEditDialog(this);
            taskEditDialog.OnHide((modified) => { // When hiding the dialog, we update the task list data if there are changes.
                if (modified)
                {
                    if (taskEditDialog.IsNewItem) // If we created a new task, then reload the data with the current sorting parameters.
                    {
                        tasklist.ListSource = UpdateTasks(false) ?? tasklist.ListSource;
                    }
                    tasklist.NotifyDataSetChanged();
                }
            });
        }

        private void LvTasks_Click(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (string.IsNullOrEmpty(Storage.Instance.GetToken()))
            {
                View view = (View)sender;
                Snackbar.Make(view, "Need to be logged in", Snackbar.LengthLong)
                    .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
                return;
            }
            var task = tasklist.ListSource[e.Position];
            taskEditDialog.SetEditItem(task);
            taskEditDialog.Show();
        }


        private void Rb_Click(object sender, EventArgs e)
        {
            if (!(sender is RadioButton))
                return;
            var rb = sender as RadioButton;
            switch (rb.Id) // Look at which radio button was clicked
            {
                case Resource.Id.rbSortId:
                    if (orderBy == "id") // If it was pressed earlier, then we change the direction to the opposite
                    {
                        orderDesc = !orderDesc;
                    }
                    else // Otherwise, change the sort field and direction to ascending
                    {
                        orderBy = "id";
                        orderDesc = false;
                    }
                    break;
                case Resource.Id.rbSortUsename:
                    if (orderBy == "username")
                    {
                        orderDesc = !orderDesc;
                    }
                    else
                    {
                        orderBy = "username";
                        orderDesc = false;
                    }
                    break;
                case Resource.Id.rbSortEmail:
                    if (orderBy == "email")
                    {
                        orderDesc = !orderDesc;
                    }
                    else
                    {
                        orderBy = "email";
                        orderDesc = false;
                    }
                    break;
                case Resource.Id.rbSortStatus:
                    if (orderBy == "status")
                    {
                        orderDesc = !orderDesc;
                    }
                    else
                    {
                        orderBy = "status";
                        orderDesc = false;
                    }
                    break;
            }

            string s = rb.Text.Trim('↑', '↓');
            s += orderDesc ? '↓' : '↑';
            rb.Text = s;

            tasklist.ListSource = UpdateTasks(false) ?? tasklist.ListSource;
        }


        private void FabPrev_Click(object sender, EventArgs e)
        {
            if (page <= 1)
            {
                Toast.MakeText(this, "That is a first page", ToastLength.Long).Show();
                return;
            }
            page--;
            tasklist.ListSource = UpdateTasks(false) ?? tasklist.ListSource;
            if (page <= 1)
                FindViewById<FloatingActionButton>(Resource.Id.fabPrevPage).Visibility = ViewStates.Invisible;
            if (page < (total_items / 3 + ((total_items % 3 > 0) ? 1 : 0)))
                FindViewById<FloatingActionButton>(Resource.Id.fabNextPage).Visibility = ViewStates.Visible;
        }

        private void FabNext_Click(object sender, EventArgs e)
        {
            if (page >= (total_items / 3 + ((total_items % 3 > 0) ? 1 : 0)))
            {
                Toast.MakeText(this, "That is a last page", ToastLength.Long).Show();
                return;
            }
            page++;
            tasklist.ListSource = UpdateTasks(false) ?? tasklist.ListSource;
            if (page >= (total_items / 3 + ((total_items % 3 > 0) ? 1 : 0)))
                FindViewById<FloatingActionButton>(Resource.Id.fabNextPage).Visibility = ViewStates.Invisible;
            if (page > 1)
                FindViewById<FloatingActionButton>(Resource.Id.fabPrevPage).Visibility = ViewStates.Visible;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            // Save links to menu items for future use
            mnuAuth = menu.GetItem(0);
            mnuLogoff = menu.GetItem(1);

            // And immediately set the active item, depending on the data on the saved authorization. In this case, 2 independent items are used for entry / exit actions.
            if (string.IsNullOrEmpty(Storage.Instance.GetToken()))
            {
                mnuAuth.SetVisible(true);
                mnuLogoff.SetVisible(false);
            }
            else
            {
                mnuAuth.SetVisible(false);
                mnuLogoff.SetVisible(true);
            }

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_auth)
            {
                authDialog.Show();
                return true;
            }
            else if (id == Resource.Id.action_logoff)
            {

                Storage.Instance.ResetAuthToken();
                Toast.MakeText(this, "Deauthed...", ToastLength.Long).Show();
                mnuAuth.SetVisible(true);
                mnuLogoff.SetVisible(false);
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            taskEditDialog.SetEditItem(null);
            taskEditDialog.Show();
        }

        List<TaskData> UpdateTasks(bool bExitOnFail)
        {

            var tasks = ApiLayer.GetTasks(page, orderBy, orderDesc);
            if (tasks.result != "ok")
            {
                Toast.MakeText(this, tasks.result, ToastLength.Long).Show();
                if (bExitOnFail) // If the bExitOnFail flag is set and there is no connection or an error has occurred, wait 5 seconds and exit.
                {
                    System.Threading.Thread.Sleep(5000);
                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                }
                return null;
            }

            total_items = tasks.total_task_count;
            if (total_items < 3)
                FindViewById<FloatingActionButton>(Resource.Id.fabNextPage).Visibility = ViewStates.Invisible;
            else
                FindViewById<FloatingActionButton>(Resource.Id.fabNextPage).Visibility = ViewStates.Visible;
            return tasks.tasks;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


    }
}
