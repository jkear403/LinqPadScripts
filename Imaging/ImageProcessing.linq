<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.Formatters.Soap.dll</Reference>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

/// <summary>
/// 	Processes an image so that it will convert
///		the image to grayscale, find the edges of
///		document and then adjust the image to make
///		teh document more readable.
/// </summary>
void Main()
{
	var openFile = new OpenFileDialog();
	openFile.CheckFileExists = true;
	openFile.CheckFileExists = true;
	openFile.DefaultExt = "*.jpg";

	var result = openFile.ShowDialog();
	if (result == DialogResult.OK)
	{
		Bitmap inBmp = new Bitmap(openFile.FileName);
		Bitmap outBmp = null;

		try
		{
			Bitmap tmp1 = inBmp;
			decimal scaleRatio = DetermineBestScale(250, 250, tmp1);
			
			// Converts to Grayscale
			tmp1 = ConvertToGrayscale(tmp1);
			
			// Shrinks the image
			tmp1 = ResizeImage(tmp1, scaleRatio);

			// Prints the Grayscale image
			// ONLY for Linqpad
			tmp1.Dump();
			
			// Detects the corner points of the document in the image
			List<PointF> points = AutoDetectDocument(tmp1);
			List<PointF> scaledPoints = new List<PointF>();

			foreach (PointF p in points)
			{
				decimal scaledx = (decimal)p.X / scaleRatio;
				decimal scaledy = (decimal)p.Y / scaleRatio;
				PointF scaledPoint = new PointF((float)scaledx, 
												(float)scaledy);
				scaledPoints.Add(scaledPoint);
			}
			outBmp = AdjustImage(inBmp, scaledPoints);
			outBmp = ResizeImage(outBmp, 
								 DetermineBestScale(500, 500, outBmp));

			// Prints the Final image
			// ONLY for Linqpad
			outBmp.Dump();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message + 
						Environment.NewLine + 
						ex.StackTrace);
		}
	}

}

private decimal DetermineBestScale(int newHeight, int newWidth, Bitmap original)
{
	decimal scaleHeight = (decimal)newHeight / (decimal)original.Height;
	decimal scaleWidth = (decimal)newWidth / (decimal)original.Width;
	return Math.Min(scaleHeight, scaleWidth);
}

private Bitmap ResizeImage(Bitmap original, decimal scale)
{
	return new Bitmap(original, 
					  (int)(original.Width * scale), 
					  (int)(original.Height * scale));
}

/// <summary>
/// 	Basic logic behind this is to find the center point
///		and steps up the document to find the top edge. Steps
///		down to find the bottom edge. While going up and down
///		steps to the left and right to find the sides of the
///		document and ultimately the four corners.
/// </summary>
private List<PointF> AutoDetectDocument(Bitmap bmp)
{
	List<PointF> documentCorners = new List<System.Drawing.PointF>();
	int centerX = bmp.Width / 2;
	int centerY = bmp.Height / 2;
	PointF centerPt = new PointF(centerX, centerY);

	PointF ptTop1 = FindTopEdgePoint(centerX, centerY, 0, bmp);
	PointF ptBottom1 = FindBottomEdgePoint(centerX, centerY, 0, bmp);
	
	PointF tl = new PointF();
	bool foundTl = false;
	
	PointF tr = new PointF();
	bool foundTr = false;
	
	PointF bl = new PointF();
	bool foundBl = false;
	
	PointF br = new PointF();
	bool foundBr = false;
	
	// Finds the top left and top right corners of the document
	for (int y = Convert.ToInt32(ptTop1.Y)+1; 
			 y < Convert.ToInt32(ptBottom1.Y)-1; y++)
	{
		PointF ptLeft = FindLeftEdgePoint(centerX, y, 0, bmp);
		PointF ptRight = FindRightEdgePoint(centerX, y, 0, bmp);
		
		if (y == Convert.ToInt32(ptTop1.Y) + 1 || !foundTl)
		{
			if (ptLeft.X != 0)
			{
				tl = ptLeft;
				foundTl = true;
			}
		}

		if (y == Convert.ToInt32(ptTop1.Y) + 1 || !foundTr)
		{
			if (ptRight.X != bmp.Width)
			{
				tr = ptRight;
				foundTr = true;
			}
		}
	}

	// Finds the bottom left and bottom right corners of the document
	for (int y = Convert.ToInt32(ptBottom1.Y)-1; 
			 y > Convert.ToInt32(ptTop1.Y)+1; y--)
	{
		PointF ptLeft = FindLeftEdgePoint(centerX, y, 0, bmp);
		PointF ptRight = FindRightEdgePoint(centerX, y, 0, bmp);

		if (y == Convert.ToInt32(ptBottom1.Y) - 1 || !foundBl)
		{
			if (ptLeft.X != 0)
			{			
				bl = ptLeft;
				foundBl = true;
			}
		}

		if (y == Convert.ToInt32(ptBottom1.Y) - 1 || !foundBr)
		{
			if (ptRight.X != bmp.Width)
			{
				br = ptRight;
				foundBr = true;
			}
		}
	}

	// Adds the Red Lines overlapping the document
	for (int y = Convert.ToInt32(ptTop1.Y)+1; 
			 y < Convert.ToInt32(ptBottom1.Y)-1; y++)
	{
		PointF ptLeft = FindLeftEdgePoint(centerX, y, 0, bmp);
		PointF ptRight = FindRightEdgePoint(centerX, y, 0, bmp);
		using (Graphics g = Graphics.FromImage(bmp))
		{
			ptLeft.X = ptLeft.X + 1;
			ptRight.X = ptRight.X - 1;
			g.DrawLine(new Pen(Color.Red), ptLeft, ptRight);
		}
	}

	// Prints an image with Red Lines overlapping the document
	// ONLY for Linqpad
	bmp.Dump();

	// Adds the document corners to the list
	documentCorners.Add(tr);
	documentCorners.Add(br);
	documentCorners.Add(bl);
	documentCorners.Add(tl);
	
	return documentCorners;
}

private PointF FindTopEdgePoint(int x, int y, float slope, Bitmap bmp)
{
	Color prevColor = GetColor(x, y, bmp);
	Color nextColor = GetColor(x, y, bmp);
	int deviationFailed = 0;
	PointF returnPoint = new PointF(x, 0);
	for (int u = y; u >= 0; u--)
	{
		deviationFailed = 0;
		if (u != 0)
		{ nextColor = GetColor(x, u--, bmp); }
		Color currentColor = GetColor(x, u, bmp);
		if (!CheckDeviation(prevColor, currentColor, nextColor))
		{
			for (var d = 0; d <= 10; d++)
			{
				if (u - d >= 0)
				{
					Color nextDeviationCheck = GetColor(x, u - d, bmp);
					if (!CheckDeviation(prevColor, prevColor, nextDeviationCheck))
					{
						deviationFailed++;
					}
				}
			}
		}

		prevColor = currentColor;

		if (deviationFailed >= 4)
		{
			returnPoint = new PointF(x, u);
		}
	}
	
	return returnPoint;
}

private PointF FindLeftEdgePoint(int x, int y, float slope, Bitmap bmp)
{
	Color prevColor = GetColor(x, y, bmp);
	Color nextColor = GetColor(x, y, bmp);
	int deviationFailed = 0;
	PointF returnPoint = new PointF(0, y);
	for (int l = x; l >= 0; l--)
	{
		float actualY = y;
		
		deviationFailed = 0;
		if (l != 0)
		{ nextColor = GetColor(l--, actualY, bmp); }
		Color currentColor = GetColor(l, actualY, bmp);
		if (!CheckDeviation(prevColor, currentColor, nextColor))
		{
			for (var d = 0; d <= 10; d++)
			{
				if (l - d >= 0)
				{
					Color nextDeviationCheck = GetColor(l - d, actualY, bmp);
					if (!CheckDeviation(prevColor, prevColor, nextDeviationCheck))
					{
						deviationFailed++;
					}
				}
			}
		}

		prevColor = currentColor;

		if (deviationFailed >= 4)
		{
			returnPoint = new PointF(l, actualY);
		}
	}

	return returnPoint;
}

private PointF FindRightEdgePoint(int x, int y, float slope, Bitmap bmp)
{
	Color prevColor = GetColor(x, y, bmp);
	Color nextColor = GetColor(x, y, bmp);
	int deviationFailed = 0;
	PointF returnPoint = new PointF(bmp.Width, y);
	for (int l = x; l <= bmp.Width - 2; l++)
	{
		deviationFailed = 0;
		if (l != bmp.Width - 2)
		{ nextColor = GetColor(l++, y, bmp); }
		Color currentColor = GetColor(l, y, bmp);
		if (!CheckDeviation(prevColor, currentColor, nextColor))
		{
			for (var d = 0; d <= 10; d++)
			{
				if (l + d < bmp.Width - 1)
				{
					Color nextDeviationCheck = GetColor(l + d, y, bmp);
					if (!CheckDeviation(prevColor, prevColor, nextDeviationCheck))
					{
						deviationFailed++;
					}
				}
			}
		} else if (l == bmp.Width - 1)
		{
			returnPoint = new PointF(l, y);
		}

		prevColor = currentColor;

		if (deviationFailed >= 3)
		{
			returnPoint = new PointF(l, y);
		}
	}
	return returnPoint;
}

private PointF FindBottomEdgePoint(int x, int y, float slope, Bitmap bmp)
{
	Color prevColor = GetColor(x, y, bmp);
	Color nextColor = GetColor(x, y, bmp);
	int deviationFailed = 0;
	PointF returnPoint = new PointF(x, bmp.Height);
	for (int u = y; u <= bmp.Height - 2; u++)
	{
		deviationFailed = 0;
		if (u != bmp.Height - 2)
		{ nextColor = GetColor(x, u++, bmp); }
		
		Color currentColor = GetColor(x, u, bmp);
		if (!CheckDeviation(prevColor, currentColor, nextColor))
		{
			for (var d = 0; d <= 10; d++)
			{
				if (u + d < bmp.Height - 1)
				{
					Color nextDeviationCheck = GetColor(x, u + d, bmp);
					if (!CheckDeviation(prevColor, prevColor, nextDeviationCheck))
					{
						deviationFailed++;
					}
				}
			}
		}

		prevColor = currentColor;

		if (deviationFailed >= 5)
		{
			returnPoint = new PointF(x, u);
		}
	}

	return returnPoint;
}

/// <summary>
/// 	Checks to see if the deviation in the color of
///		the current pixel, previous pixel, and the
///		next pixel.
/// </summary>
private bool CheckDeviation(Color p, Color c, Color n)
{
	int dl = 2;
	int ds = 13;
	bool rFailed = false;
	bool gFailed = false;
	bool bFailed = false;

	//Check R
	if (c.R < p.R - dl || n.R < p.R - dl)
	{
		if (n.R < c.R - ds || n.R > c.R + ds)
		{
			rFailed = true;
		}
	}

	
	//Check G
	if (c.G < p.G - dl || n.G < p.G - dl)
	{
		if (n.G < c.G - ds || n.G > c.G + ds)
		{
			gFailed = true;
		}
	}
	
	//Check B
	if (c.B < p.B - dl || n.B < p.B - dl)
	{
		if (n.B < c.B - ds || n.B > c.B + ds)
		{
			bFailed = true;
		}
	}
	
	int failed = 0;
	if (rFailed) { failed++; }
	if (gFailed) { failed++; }
	if (bFailed) { failed++; }
	
	if (failed >= 1)
	{
		return false;
	}
	return true;
}

/// <summary>
/// 	This function will adjust the images pixels so that
///		the document corners matches the bitmaps corners.
///		Moves each pixel in the document to the correct position
///		of the adjusted iamge.
/// </summary>
private Bitmap AdjustImage(Bitmap bmp, List<PointF> points)
{
	int oWidth = bmp.Width;
	int oHeight = bmp.Height;
	PointF TL = new PointF(0, 0);
	PointF TR = new PointF(0, 0);
	PointF BL = new PointF(0, 0);
	PointF BR = new PointF(0, 0);

	List<PointF> sortPoints = points.AsQueryable().OrderBy(p => p.Y).ToList();

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

/// <summary>
/// 	Looks up the pixel and averages the colors of
///		the pixels around the center
/// </summary>
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
	{ r = 254.5; }

	if (b >= 255)
	{ b = 254.5; }

	if (g >= 255)
	{ g = 254.5; }

	Color c = Color.FromArgb(Convert.ToInt32(r + 0.5), 
							 Convert.ToInt32(g + 0.5), 
							 Convert.ToInt32(b + 0.5));

	return c;
}

/// <summary>
/// 	Takes an image and converts it to the Grayscale
///		using a color matrix
/// </summary>
private Bitmap ConvertToGrayscale(Bitmap bmp)
{
	using (Graphics gr = Graphics.FromImage(bmp))
	{
		var gray_matrix = new float[][] {
				new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
				new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
				new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
				new float[] { 0,      0,      0,      1, 0 },
				new float[] { 0,      0,      0,      0, 1 }
			};

		var ia = new System.Drawing.Imaging.ImageAttributes();
		ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(gray_matrix));
		var rc = new Rectangle(0, 0, bmp.Width, bmp.Height);
		gr.DrawImage(bmp, rc, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, ia);
	}

	return bmp;
}