using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math.Geometry;

namespace OLAF.Services.Classifiers
{
    public class BlobDetector : Service<ImageArtifact, ImageArtifact>
    {
        public BlobDetector(Profile profile) : base(profile)
        {

        }

        public override ApiResult Init()
        {
            if (Status != ApiStatus.Initializing)
            {
                return SetErrorStatusAndReturnFailure();
            }
            else
            {
                return SetInitializedStatusAndReturnSucces();
            }
        }

        protected override ApiResult ProcessClientQueueMessage(ImageArtifact message)
        {
            BlobCounter blobCounter = new BlobCounter();
            Blob[] blobs;

            Dictionary<int, List<IntPoint>> leftEdges = new Dictionary<int, List<IntPoint>>();
            Dictionary<int, List<IntPoint>> rightEdges = new Dictionary<int, List<IntPoint>>();
            Dictionary<int, List<IntPoint>> topEdges = new Dictionary<int, List<IntPoint>>();
            Dictionary<int, List<IntPoint>> bottomEdges = new Dictionary<int, List<IntPoint>>();

            Dictionary<int, List<IntPoint>> hulls = new Dictionary<int, List<IntPoint>>();
            Dictionary<int, List<IntPoint>> quadrilaterals = new Dictionary<int, List<IntPoint>>();


            Bitmap image = message.HasBitmap ? Accord.Imaging.Image.Clone(message.Image) :
                Accord.Imaging.Image.FromFile(message.FileArtifact.Path);

            BitmapData bitmapData = image.LockBits(ImageLockMode.ReadWrite);

            // step 1 - turn background to black
            ColorFiltering colorFilter = new ColorFiltering();
            //
            colorFilter.Red = new IntRange(0, 64);
            colorFilter.Green = new IntRange(0, 64);
            colorFilter.Blue = new IntRange(0, 64);
            colorFilter.FillOutsideRange = false;

            colorFilter.ApplyInPlace(bitmapData);


            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 25;
            blobCounter.MinWidth = 25;

            //blobCounter.BackgroundThreshold = 5;
            blobCounter.ProcessImage(bitmapData);
            
            blobs = blobCounter.GetObjectsInformation();
            Debug("Detected {0} shapes.", blobs.Length);
            image.UnlockBits(bitmapData);
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            GrahamConvexHull grahamScan = new GrahamConvexHull();

           
            
            Pen redPen = new Pen(Color.Red, 2);       // quadrilateral
             // triangle

            Graphics g = Graphics.FromImage(image);
            for (int i = 0; i < blobs.Length; i++)

            {

                
                g.DrawRectangle(redPen, blobs[i].Rectangle);

                #region Experiments
                /*
                    Accord.Point center;
                float radius;

                // is circle ?
                if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                {
                    g.DrawEllipse(yellowPen,
                        (float)(center.X - radius), (float)(center.Y - radius),
                        (float)(radius * 2), (float)(radius * 2));
                }
                else
                {
                    g.DrawPolygon(redPen, ToPointsArray(edgePoints));
                    

                    
                }
                */
            }

            /*
            List<IntPoint> leftEdge = new List<IntPoint>();
            List<IntPoint> rightEdge = new List<IntPoint>();
            List<IntPoint> topEdge = new List<IntPoint>();
            List<IntPoint> bottomEdge = new List<IntPoint>();

            // collect edge points
            blobCounter.GetBlobsLeftAndRightEdges(blob, out leftEdge, out rightEdge);
            blobCounter.GetBlobsTopAndBottomEdges(blob, out topEdge, out bottomEdge);

            leftEdges.Add(blob.ID, leftEdge);
            rightEdges.Add(blob.ID, rightEdge);
            topEdges.Add(blob.ID, topEdge);
            bottomEdges.Add(blob.ID, bottomEdge);

            // find convex hull
            List<IntPoint> edgePoints = new List<IntPoint>();
            edgePoints.AddRange(leftEdge);
            edgePoints.AddRange(rightEdge);

            List<IntPoint> hull = grahamScan.FindHull(edgePoints);
            hulls.Add(blob.ID, hull);

            List<IntPoint> quadrilateral = null;

            // find quadrilateral
            if (hull.Count < 4)
            {
                quadrilateral = new List<IntPoint>(hull);
            }
            else
            {
                quadrilateral = PointsCloud.FindQuadrilateralCorners(hull);
            }
            quadrilaterals.Add(blob.ID, quadrilateral);

        }
        using (Graphics g = Graphics.FromImage(image))
        {
            //foreach (var qq in quadrilaterals.Values)
            //{
            //    DrawEdge(g, highlightPen, qq);
            //}

        }
        */
            #endregion
                image.Save(GetLogDirectoryPathTo("shapes_{0}.bmp".F(message.Id))); return ApiResult.Success;
            
        }

        private System.Drawing.Point[] ToPointsArray(List<IntPoint> points)
        {
            System.Drawing.Point[] array = new System.Drawing.Point[points.Count];

            for (int i = 0; i < points.Count; i++)
                array[i] = new System.Drawing.Point(points[i].X, points[i].Y);

            return array;
        }

        // Convert list of AForge.NET's IntPoint to array of .NET's Point
        private static System.Drawing.Point[] PointsListToArray(List<IntPoint> list)
        {
            System.Drawing.Point[] array = new System.Drawing.Point[list.Count];

            for (int i = 0, n = list.Count; i < n; i++)
            {
                array[i] = new System.Drawing.Point(list[i].X, list[i].Y);
            }

            return array;
        }

        // Draw object's edge
        private static void DrawEdge(Graphics g, Pen pen, List<IntPoint> edge)
        {
            System.Drawing.Point[] points = PointsListToArray(edge);

            if (points.Length > 1)
            {
                g.DrawPolygon(pen, points);
            }
            else
            {
                g.DrawLine(pen, points[0], points[0]);
            }
        }
    }
}
