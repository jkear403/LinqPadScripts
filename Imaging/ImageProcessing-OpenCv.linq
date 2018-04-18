<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.Formatters.Soap.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <NuGetReference>EMGU.CV</NuGetReference>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <Namespace>Emgu.CV</Namespace>
  <Namespace>Emgu.CV.Structure</Namespace>
  <Namespace>Emgu.CV.Util</Namespace>
  <Namespace>Emgu.CV.CvEnum</Namespace>
</Query>

public PointF[] rect;
public int lowerThreshold;
public int upperThreshold;
VectorOfPoint largestContour;
VectorOfVectorOfPoint myContours = new VectorOfVectorOfPoint();

void Main()
{
	Execute();
}

// order_points() takes an input of VectorOfPoint and returns an ordered rectangle as a result
// called by four_point_transform()
public PointF[] order_points(VectorOfPoint vp)
{
	rect = new PointF[4];
	List<Point> sortPoints = vp.ToArray().AsQueryable().OrderBy(p => p.Y).ToList();
	PointF TL = new Point(0, 0);
	PointF TR = new Point(0, 0);
	PointF BL = new Point(0, 0);
	PointF BR = new Point(0, 0);
	
	if (sortPoints[0].X < sortPoints[1].X)
	{
		TL = sortPoints[0];
		TR = sortPoints[1];
	}
	else
	{
		TL = sortPoints[1];
		TR = sortPoints[0];
	}

	if (sortPoints[2].X < sortPoints[3].X)
	{
		BL = sortPoints[2];
		BR = sortPoints[3];
	}
	else
	{
		BL = sortPoints[3];
		BR = sortPoints[2];
	}
	
	rect[0] = TL;
	rect[2] = BR;
	rect[3] = TR;
	rect[1] = BL;
//	var sumpoints = vp.ToArray().Select(p => p.X + p.Y);
//	rect[0] = vp[sumpoints.ToList().IndexOf(sumpoints.Min())];
//	rect[2] = vp[sumpoints.ToList().IndexOf(sumpoints.Max())];
//
//	var diffpoints = vp.ToArray().Select(p => p.X - p.Y);
//	rect[1] = vp[diffpoints.ToList().IndexOf(diffpoints.Min())];
//	rect[3] = vp[diffpoints.ToList().IndexOf(diffpoints.Max())];

	return rect;
}

// -------------------------------------------------------------------------------------
private Bitmap AdjustImage(Bitmap bmp, List<Point> points)
{
	int oWidth = bmp.Width;
	int oHeight = bmp.Height;
	PointF TL = new PointF(0, 0);
	PointF TR = new PointF(0, 0);
	PointF BL = new PointF(0, 0);
	PointF BR = new PointF(0, 0);

	List<Point> sortPoints = points.AsQueryable().OrderBy(p => p.Y).ToList();

	if (sortPoints[0].X < sortPoints[1].X)
	{
		TL = sortPoints[0];
		TR = sortPoints[1];
	}
	else
	{
		TL = sortPoints[1];
		TR = sortPoints[0];
	}

	if (sortPoints[2].X < sortPoints[3].X)
	{
		BL = sortPoints[2];
		BR = sortPoints[3];
	}
	else
	{
		BL = sortPoints[3];
		BR = sortPoints[2];
	}


	Bitmap outBmp = new Bitmap(oWidth, oHeight);
	for (int x = 0; x <= oWidth - 1; x++)
	{
		for (int y = 0; y <= oHeight - 1; y++)
		{
			double rx = Convert.ToDouble((double)x / (double)oWidth);
			double ry = Convert.ToDouble((double)y / (double)oHeight);

			double topX = TL.X + rx * (TR.X - TL.X);
			double topY = TL.Y + rx * (TR.Y - TL.Y);
			double bottomX = BL.X + rx * (BR.X - BL.X);
			double bottomY = BL.Y + rx * (BR.Y - BL.Y);

			double centerX = topX + ry * (bottomX - topX);
			double centerY = topY + ry * (bottomY - topY);

			Color c = GetColor(centerX, centerY, bmp);
			outBmp.SetPixel(x, y, c);
		}
	}

	return outBmp;

}

private Color GetColor(double x, double y, Bitmap bmp)
{
	double xFrac = x - Convert.ToInt32(x);
	double yFrac = y - Convert.ToInt32(y);

	Color cBL = bmp.GetPixel(Convert.ToInt32(x + 1), Convert.ToInt32(y + 1));
	Color cCB = bmp.GetPixel(Convert.ToInt32(x + 0), Convert.ToInt32(y + 1));
	Color cCR = bmp.GetPixel(Convert.ToInt32(x + 1), Convert.ToInt32(y + 0));
	Color cOR = bmp.GetPixel(Convert.ToInt32(x + 0), Convert.ToInt32(y + 0));

	double dBL = Math.Sqrt(xFrac * xFrac + yFrac * yFrac);
	double dCB = Math.Sqrt((1 - xFrac) * (1 - xFrac) + yFrac * yFrac);
	double dCR = Math.Sqrt(xFrac * xFrac + (1 - yFrac) * (1 - yFrac));
	double dOR = Math.Sqrt((1 - xFrac) * (1 - xFrac) + (1 - yFrac) * (1 - yFrac));

	double factor = 1.0f / (dBL + dCB + dCR + dOR);
	dBL *= factor;
	dCB *= factor;
	dCR *= factor;
	dOR *= factor;

	double r = dBL * cBL.R + dCB * cCB.R + dCR * cCR.R + dOR * cOR.R;
	double g = dBL * cBL.G + dCB * cCB.G + dCR * cCR.G + dOR * cOR.G;
	double b = dBL * cBL.B + dCB * cCB.B + dCR * cCR.B + dOR * cOR.B;

	if (r >= 255)
	{
		r = 254.5;
	}

	if (b >= 255)
	{
		b = 254.5;
	}

	if (g >= 255)
	{
		g = 254.5;
	}

	Color c = Color.FromArgb(Convert.ToInt32(r + 0.5), Convert.ToInt32(g + 0.5), Convert.ToInt32(b + 0.5));

	return c;
}
// -------------------------------------------------------------------------------------


// four_point_transform() takes an image and a VectorOfPoint and warps the image based on the rectangle
// extracted from the Vector
public Emgu.CV.Image<Gray, byte> four_point_transform(Emgu.CV.Image<Bgr, byte> img, VectorOfPoint vp)
{
	PointF[] rect = order_points(vp);

	var tl = rect[0];
	var br = rect[2];
	var tr = rect[3];
	var bl = rect[1];
	rect.Dump();
	var widthA = Math.Sqrt((Math.Pow((br.X - bl.X), 2d) + Math.Pow((br.Y - bl.Y), 2d)));
	var widthB = Math.Sqrt((Math.Pow((tr.X - tl.X), 2d) + Math.Pow((tr.Y - tl.Y), 2d)));

	var maxWidth = Math.Min((int)widthA, (int)widthB);
	Console.WriteLine("Width=" + maxWidth);
	var heightA = Math.Sqrt((Math.Pow((tr.X - br.X), 2d) + Math.Pow((tr.Y - br.Y), 2d)));
	var heightB = Math.Sqrt((Math.Pow((tl.X - bl.X), 2d) + Math.Pow((tl.Y - bl.Y), 2d)));

	var maxHeight = Math.Min((int)heightA, (int)heightB);
	Console.WriteLine("Height=" + maxHeight);
	PointF[] dst = { new PointF(0, 0), new PointF(0, maxHeight - 1), new PointF(maxWidth - 1, maxHeight - 1), new PointF(maxWidth - 1, 0) };

	var M = CvInvoke.GetPerspectiveTransform(rect, dst);
	var warped = new Emgu.CV.Image<Gray, byte>(new Size(maxWidth, maxHeight));
	CvInvoke.WarpPerspective(img, warped, M, new Size(maxWidth, maxHeight));
	
	DumpImage(warped);
	
	return warped;
}

// helper function:
// finds a cosine of angle between vectors
// from pt0->pt1 and from pt0->pt2
static double angle(Point pt1, Point pt2, Point pt0)
{
	double dx1 = pt1.X - pt0.X;
	double dy1 = pt1.Y - pt0.Y;
	double dx2 = pt2.X - pt0.X;
	double dy2 = pt2.Y - pt0.Y;
	return (dx1 * dx2 + dy1 * dy2) / Math.Sqrt((dx1 * dx1 + dy1 * dy1) * (dx2 * dx2 + dy2 * dy2) + 1e-10);
}

private void Execute()
{
	// Reset largest contour from last run
	largestContour = null;

	// Reset contours from last run
	myContours = new VectorOfVectorOfPoint();

	// Prompt for image
	var x = new OpenFileDialog();
	x.CheckFileExists = true;
	x.CheckFileExists = true;
	x.DefaultExt = "*.jpg";

	var result = x.ShowDialog();
	if (result == DialogResult.OK)
	{

		// Open specified image
		var image = new Image<Bgr, byte>(x.FileName);

		// Copy the original image for backup
		Emgu.CV.Image<Bgr, byte> original = new Image<Bgr, byte>(image.Size);
		image.CopyTo(original);

		// For Debug Display -- Remove Me for production 
		DumpImage(original.Bitmap);
		DumpImage(image.Bitmap);

		// Definitions for image conversions
		Emgu.CV.Image<Gray, byte> grayScaleImage = new Image<Gray, byte>(image.Size);
		Emgu.CV.Image<Gray, byte> blurredImage = new Image<Gray, byte>(image.Size);
		Emgu.CV.Mat gray = new Emgu.CV.Mat(image.Size, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
		Emgu.CV.Mat gray0 = new Emgu.CV.Mat(blurredImage.Size, Emgu.CV.CvEnum.DepthType.Cv8U, 3);

		// Blur image
		CvInvoke.MedianBlur(image, blurredImage, 9);

		// find squares in every color plane of the image
		for (int c = 0; c < 3; c++)
		{
			int[] ch = { c, 0 };
			CvInvoke.MixChannels(blurredImage, gray0, ch);

			// Try several thresholds
			const int threshold_level = 2;
			Emgu.CV.Image<Gray, byte> newGray;

			for (int l = 0; l < threshold_level; l++)
			{
				// Use Canny instead of zero threshold level!
				// Canny will catch squares with gradient shading
				if (l == 0)
				{
					// Emgu missing aperature setting which should be 3
					CvInvoke.Canny(gray0, gray, 10, 20);

					// Dilate helps remove potential holes between edge segments
					CvInvoke.Dilate(gray, gray, new Mat(), new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
					newGray = new Image<Gray, byte>(gray.Bitmap);
				}
				else
				{
					newGray = new Image<Gray, byte>(gray.Bitmap);
					newGray.Convert<Single>(delegate (Byte b) { return (Single)Math.Cos(b * b / 255.0); });
				}

				// Find contours
				VectorOfVectorOfPoint contours2 = new VectorOfVectorOfPoint();
				VectorOfPoint approx = new VectorOfPoint();

				CvInvoke.FindContours(newGray, contours2, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

				for (int i = 0; i < contours2.Size; i++)
				{
					// approximate contour with accuracy proportional
					// to the contour perimeter
					CvInvoke.ApproxPolyDP(contours2[i], approx, CvInvoke.ArcLength(contours2[i], true) * 0.02, true);

					// Note: absolute value of an area is used because
					// area may be positi
					// area may be positive or negative - in accordance with the
					// contour orientation
					if (approx.Size == 4)
					{
						double maxCosine = 0;

						for (int j = 2; j < 5; j++)
						{
							double cosine = Math.Abs(angle(approx[j % 4], approx[j - 2], approx[j - 1]));
							maxCosine = Math.Max(maxCosine, cosine);
						}

						if (maxCosine < 0.3)
						{
							myContours.Push(approx);
						}
					}
				}
			}
		}

		// For Debug Display -- Remove Me for production 
		DumpImage(grayScaleImage);
		DumpImage(blurredImage);

		int count = myContours.Size;
		int largest_contour_index = 0;
		double largest_area = 0;
		double brightestContour = 0;


		// Fucking irritating way to do this 
		Dictionary<int, double> areas = new Dictionary<int, double>();

		if (count > 0)
		{
			for (int i2 = 0; i2 < count; i2++)
			{
				using (VectorOfPoint contour2 = myContours[i2])
				{

					PointF[] rect = order_points(contour2);

					var tl2 = rect[0];
					var br2 = rect[2];
					var tr2 = rect[3];
					var bl2 = rect[1];

					// Area of rectangle
					areas.Add(i2, (tr2.X - tl2.X) * (bl2.Y - tl2.Y));

				}
			}
		}

		areas = areas.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

		// Find largest rectangle in the dectected contours

		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				// For debug - remove for production
				MCvScalar color = new MCvScalar(0, 255, 0);
				CvInvoke.DrawContours(image, myContours, areas.ElementAt(i).Key, color, 3);

				using (VectorOfPoint contour = myContours[areas.ElementAt(i).Key])
				{
					PointF[] rect = order_points(contour);

					var tl = rect[0];
					var br = rect[2];
					var tr = rect[3];
					var bl = rect[1];

					// Area of rectangle
					double a = (tr.X - tl.X) * (bl.Y - tl.Y);

					try
					{
						// Get the top left/right, and bottom left/right pixels so we can 
						// detect color.  Reduces the size of the rectangle by 5 pixels so
						// we can ensure we're within the document
						Bgr tlpixel = original[(int)rect[0].Y + 5, (int)rect[0].X + 5];
						Bgr trpixel = original[(int)rect[3].Y + 5, (int)rect[3].X - 5];
						Bgr blpixel = original[(int)rect[1].Y - 5, (int)rect[1].X + 5];
						Bgr brpixel = original[(int)rect[2].Y - 5, (int)rect[2].X - 5];

						var b = (tlpixel.Blue + tlpixel.Red + tlpixel.Green + trpixel.Blue + trpixel.Red + trpixel.Green + blpixel.Blue + blpixel.Red + blpixel.Green + brpixel.Blue + brpixel.Red + brpixel.Green);

						// Save the area as the largest IF it is larger than the last
						// and the brightness is the highest   
						if (a > (areas.ElementAt(0).Value * .30))
						{
							if (b > brightestContour)
							{
								brightestContour = b;
								largest_area = a;
								largest_contour_index = areas.ElementAt(i).Key;                      //Store the index of largest contour
								largestContour = contour;
							}
						}
					}
					catch { }
				}

			}


			// For debug -- remove for production
			MCvScalar color2 = new MCvScalar(0, 0, 255);
			CvInvoke.DrawContours(image, myContours, largest_contour_index, color2, 6);

			// For debug -- remove for production
			//DumpImage(warped);
			Console.WriteLine(CvInvoke.ContourArea(largestContour, false).ToString());
			Bitmap BmpInput = image.ToBitmap();
			DumpImage(BmpInput);
			
			// Warp the image based on the dectected rectangle
			var warped = four_point_transform(original, largestContour);
//			var newImage = AdjustImage(original.ToBitmap(), largestContour.ToArray().ToList());
//			Console.WriteLine("Non OpenCV");
//			DumpImage(newImage);
			// For debug -- remove for production
			Bitmap BmpWarp = warped.ToBitmap();

			// Convert to a b&w bitmap "document"                
			CvInvoke.Threshold(warped, warped, 125, 255, ThresholdType.Binary);
			CvInvoke.CvtColor(warped, warped, ColorConversion.Bgr2Gray);



			CvInvoke.CvtColor(original, original, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
		}
	}

}

private void DumpImage(Image imageDump)
{
	decimal scale = DetermineBestScale(500, 500, imageDump);
	Image resized = new Bitmap(imageDump, (int)(imageDump.Width * scale), (int)(imageDump.Height * scale));
	resized.Dump();
}

private void DumpImage(Emgu.CV.Image<Gray, byte> imageDump)
{
	Stream test = new MemoryStream(imageDump.ToJpegData());
	Image testImage = Bitmap.FromStream(test);
	DumpImage(testImage);
}

private decimal DetermineBestScale(int newHeight, int newWidth, Image original)
{
	decimal scaleHeight = (decimal)newHeight / (decimal)original.Height;
	decimal scaleWidth = (decimal)newWidth / (decimal)original.Width;
	return Math.Min(scaleHeight, scaleWidth);
}
// Define other methods and classes here
