using System;
using Eto.Forms;
using Eto.Drawing;
using System.Linq;
using Pablo.Network;
using Pablo.Network.Commands;
using System.Collections.Generic;

namespace Pablo.Interface
{
	public class ChatPanel : Panel
	{
		TextArea text;
		TextBox input;
		readonly Main main;
		ListBox users;
		bool firstMessage = true;

		User CurrentUser { get { return main.Client.CurrentUser; } }

		User SelectedUser { get { return users.SelectedValue as User; } }

		public ChatPanel(Main main)
		{
			this.main = main;
			Size = new Size(200, 140);
			
			var layout = new TableLayout(2, 1);
			layout.Add(ChatPane(), 0, 0, true, true);
			layout.Add(UserPane(), 1, 0);

			Content = layout;
			
			main.Client.Message += (sender, ce) => AddMessage(ce.Message, ce.User);
			
			main.Client.UsersChanged += delegate
			{
				var usersList = main.Client.Users.ToList();
				usersList.Sort(delegate(User x, User y)
				{
					var l = -x.Level.CompareTo(y.Level);
					return l != 0 ? l : string.Compare(x.Alias, y.Alias, StringComparison.CurrentCultureIgnoreCase);
				});
				users.Items.Clear();
				users.Items.AddRange(usersList);
			};
		}

		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			text.Append(string.Empty, true);
		}

		public bool SetChatFocus()
		{
			if (!input.HasFocus)
			{
				input.Focus();
				return true;
			}
			return false;
		}

		Control ChatPane()
		{
			var layout = new TableLayout(1, 2);
			layout.Padding = Padding.Empty;
			layout.Add(TextControl(), 0, 0, true, true);
			layout.Add(InputControl(), 0, 1);
			return layout;
		}

		Control UserPane()
		{
			users = new ListBox { Size = new Size(155, 100) };
			var menu = new ContextMenu();
			
			var kick = KickItem();
			menu.MenuItems.Add(kick);

			var level = LevelItem();
			menu.MenuItems.Add(level);
			
			users.SelectedIndexChanged += delegate
			{
				bool val = CurrentUser.Level == UserLevel.Operator && SelectedUser != null && SelectedUser.Key != main.Client.CurrentUser.Key;
				kick.Enabled = level.Enabled = val;
			};
			
			users.ContextMenu = menu;
			return users;
		}

		ImageMenuItem KickItem()
		{
			var item = new ImageMenuItem
			{
				Text = "Kick",
				Enabled = false
			};
			item.Click += delegate
			{
				main.Client.SendCommand(new UserStatusChanged { Status = UserStatus.Kicked, User = SelectedUser });
			};
			return item;
		}

		ImageMenuItem LevelItem()
		{
			var item = new ImageMenuItem
			{
				Text = "Set Level",
				Enabled = false
			};
			
			foreach (var lvl in Enum.GetNames (typeof(UserLevel)))
			{
				var lvlitem = new ImageMenuItem
				{
					Text = lvl,
					//Tag = Enum.Parse (typeof(UserLevel), lvl)
				};
				var userLevel = (UserLevel)Enum.Parse(typeof(UserLevel), lvl);
				lvlitem.Click += (sender, e) =>
				{
					var newuser = new User(SelectedUser);
					newuser.Level = userLevel;
					main.Client.SendCommand(new UserStatusChanged
					{
						Status = UserStatus.LevelChange,
						User = newuser
					});
				};
				item.MenuItems.Add(lvlitem);
			}
			return item;
		}

		public void AddMessage(string message, User user)
		{
			var prefix = firstMessage ? string.Empty : Environment.NewLine;
			if (user != null)
			{
				text.Append(prefix + string.Format("[{0:t}] {1}: {2}", DateTime.Now, user.Alias, message), true);
			}
			else
				text.Append(prefix + string.Format("[{0:t}] {1}", DateTime.Now, message), true);
			firstMessage = false;
		}

		Control TextControl()
		{
			var control = new TextArea
			{ 
				//Size = new Size(100, 80),
				ReadOnly = true/*,
				BackgroundColor = Color.White*/
			};

			text = control;
			return control;
		}

		Control InputControl()
		{
			var control = input = new TextBox
			{
				MaxLength = 200
			};
			control.KeyDown += (sender, e) =>
			{
				if (e.KeyData == Key.Enter)
				{
					// send over pipe!
					if (!string.IsNullOrWhiteSpace(control.Text))
					{
						main.Client.SendMessage(control.Text);
						control.Text = string.Empty;
					}
					e.Handled = true;
				}
			};
			return control;
		}
	}
}

