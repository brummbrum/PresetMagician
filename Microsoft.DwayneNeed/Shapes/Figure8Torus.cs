﻿using System;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.DwayneNeed.Numerics;

namespace Microsoft.DwayneNeed.Shapes
{
    public class Figure8Torus : ParametricShape3D
    {
        static Figure8Torus()
        {
            // So texture coordinates work out better, configure the default
            // MinV property to be PI.
            MinVProperty.OverrideMetadata(typeof(Figure8Torus), new PropertyMetadata(Math.PI));

            // So texture coordinates work out better, configure the default
            // MaxV property to be 3*PI.
            MaxVProperty.OverrideMetadata(typeof(Figure8Torus), new PropertyMetadata(Math.PI * 3.0));
        }


        public static DependencyProperty AProperty =
            DependencyProperty.Register(
                "A",
                typeof(double),
                typeof(Figure8Torus), new PropertyMetadata(
                    1.0, new PropertyChangedCallback(OnPropertyChangedAffectsModel)));

        public double A
        {
            get { return (double) GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }

        protected override Point3D Project(MemoizeMath u, MemoizeMath v)
        {
            double a = A;

            double x = u.Cos * (a + v.Sin * u.Cos - (Math.Sin(2.0 * v.Value) * u.Sin) / 2.0);
            double y = u.Sin * (a + v.Sin * u.Cos - (Math.Sin(2.0 * v.Value) * u.Sin) / 2.0);
            double z = u.Sin * v.Sin + (u.Cos * Math.Sin(2.0 * v.Value)) / 2.0;
            return new Point3D(x, y, z);
        }
    }
}