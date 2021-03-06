﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ElasticSea.Wintile.Model.Managers.Window;
using ElasticSea.Wintile.Utils;
using Rect = ElasticSea.Wintile.Model.Entities.Rect;

namespace ElasticSea.Wintile.Model.Managers.Strategies
{
    public abstract class ClosesRectToWindow
    {
        public static readonly Vector left = new Vector(-1, 0);
        public static readonly Vector right = new Vector(1, 0);
        public static readonly Vector up = new Vector(0, -1);
        public static readonly Vector down = new Vector(0, 1);

        protected readonly IWindowManager windowManager;

        protected ClosesRectToWindow(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
        }

        protected Rect GetClosest(IEnumerable<Rect> rects, Rect rect, Vector direction)
        {
            // TODO Hack, offset the rect a little bit so there are not margin problems
            var rectFixed = new Rect(rect.Left - .1f, rect.Top - .1f, rect.Right + .1f, rect.Bottom + .1f);

            return rects
                .Select(r => new {rect = r, Score = ComputeScore(direction, rectFixed, r)})
                .OrderByDescending(a => a.Score)
                .FirstOrDefault(a => a.Score > 0)?.rect;
        }

        private static double ComputeScore(Vector lookDirection, Rect original, Rect target)
        {
            var targetCenter = target.Center + target.Size.Multiply(lookDirection / 2);
            var originalCenter = original.Center + original.Size.Multiply(lookDirection / 2);
            var relativePosition = targetCenter - originalCenter;

            var distance = relativePosition.Length;
            var direction = relativePosition.Normalized();

            var dot = lookDirection * direction;
            return dot * (1 / distance);
        }
    }
}