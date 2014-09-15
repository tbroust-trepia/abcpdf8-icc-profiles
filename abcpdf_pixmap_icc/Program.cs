using System;
using WebSupergoo.ABCpdf8;
using WebSupergoo.ABCpdf8.Objects;

namespace abcpdf_pixmap_icc
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // install your own licence

            using (Doc doc = new Doc())
            {
                doc.Read("in.pdf"); // in this PDF the image has an embedded ICC profile, and I must remove them

                Console.WriteLine("################################ THROUGH GETINFO ################################");

                // Remove the ICC profiles through Get/SetInfo methods
                foreach (var item in doc.ObjectSoup)
                {
                    if (item != null && doc.GetInfo(item.ID, "/ColorSpace*[0]*:Name").Equals("ICCBased", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int profileId = doc.GetInfoInt(item.ID, "/ColorSpace*[1]:Ref"); // note the [1]: why is it there?
                        if (profileId != 0)
                        {
                            doc.GetInfo(profileId, "Decompress");
                            string profileData = doc.GetInfo(profileId, "Stream");

                            // this outputs the ICC profile raw data, with the profile's name somewhere up top
                            Console.WriteLine(string.Format("ICC profile for object ID {0}: {1}", item.ID, profileData)); 

                            doc.SetInfo(profileId, "Stream", string.Empty);
                            doc.GetInfo(profileId, "Compress");
                        }
                    }
                }

                doc.Save("out-infos.pdf");
                doc.Clear();
                doc.Read("in.pdf");

                Console.WriteLine("################################ THROUGH OBJECTS ################################");

                // Remove ICC profiles through the pixmap objects
                foreach (var item in doc.ObjectSoup)
                {
                    if (doc.GetInfo(item.ID, "Type") == "jpeg") // only work on PixMaps
                    {
                        PixMap pm = (PixMap)item;
                        if (pm.ColorSpaceType == ColorSpaceType.ICCBased)
                        {
                            // pm.ColorSpace.IccProfile is always null so I can't really set it to null or Recolor() it because it would change noting
                            Console.WriteLine(string.Format("ICC profile for object ID {0}: {1}", item.ID, pm.ColorSpace.IccProfile)); // there should already be an ICC profile (ColorSpaceType = ICCBased) so why does ColorSpace.IccProfile creates one ?
                        }
                    }
                }

                doc.Save("out-objects.pdf");
            }
        }
    }
}