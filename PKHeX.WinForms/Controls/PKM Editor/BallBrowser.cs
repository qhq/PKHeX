﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using PKHeX.Core;
using PKHeX.WinForms.Properties;

namespace PKHeX.WinForms
{
    public partial class BallBrowser : Form
    {
        public BallBrowser() => InitializeComponent();

        public int BallChoice { get; private set; } = -1;

        public void LoadBalls(Ball[] poss, ICollection<Ball> legal, IReadOnlyList<ComboItem> names)
        {
            for (int i = 0; i < poss.Length; i++)
            {
                var pb = GetBallView(poss[i], legal, names);
                flp.Controls.Add(pb);
                const int width = 5; // balls wide
                if (i % width == width - 1)
                    flp.SetFlowBreak(pb, true);
            }
        }

        private PictureBox GetBallView(Ball b, ICollection<Ball> legal, IReadOnlyList<ComboItem> names)
        {
            var img = SpriteUtil.GetBallSprite((int) b);
            var pb = new PictureBox
            {
                Size = img.Size,
                Image = img,
                BackgroundImage = legal.Contains(b) ? Resources.slotSet : Resources.slotDel,
                BackgroundImageLayout = ImageLayout.Tile
            };
            pb.MouseEnter += (_, __) => Text = names.First(z => z.Value == (int) b).Text;
            pb.Click += (_, __) => SelectBall(b);
            return pb;
        }

        private void SelectBall(Ball b)
        {
            BallChoice = (int)b;
            Close();
        }
    }
}