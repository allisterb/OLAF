using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Accord;
using Accord.Imaging;
using Accord.Math.Geometry;

namespace OLAF.Services.Classifiers
{
    public class ShapeDetector : Service<ImageArtifact, ImageArtifact>
    {
        public ShapeDetector(Profile profile) : base(profile)
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
            blobCounter.ProcessImage(image);
            blobs = blobCounter.GetObjectsInformation();
            GrahamConvexHull grahamScan = new GrahamConvexHull();

            foreach (Blob blob in blobs)
            {
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
            return ApiResult.Success;
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
                g.DrawLines(pen, points);
            }
            else
            {
                g.DrawLine(pen, points[0], points[0]);
            }
        }
    }
}
