namespace Binner.Model.Integrations.Tme
{
    /// <summary>
    /// TME Product search response
    /// </summary>
    public class ProductSearchResponse
    {
        /// <summary>
        /// Array of product objects.
        /// </summary>
        public List<TmeProduct> ProductList { get; set; } = new();

        /// <summary>
        /// Total amount of found products (pagination). PageSize is 20 rows and cannot be specified.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Actual results page number (current page number)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// List of categories in products category tree.
        /// </summary>
        public Dictionary<string, int> CategoryList { get; set; } = new();
    }

    public class TmeProduct
    {
        /// <summary>
        /// Unique product identifier
        /// </summary>
        public string? Symbol { get; set; }
        /// <summary>
        /// Producers product identifier
        /// </summary>
        public string? OriginalSymbol { get; set; }
        /// <summary>
        /// Manufacturer name
        /// </summary>
        public string? Producer { get; set; }
        /// <summary>
        /// Product description
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Leaf category id in which the product is located
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// Leaf category name in which the product is located
        /// </summary>
        public string? Category { get; set; }
        /// <summary>
        /// URL address of product image (640x480px).
        /// </summary>
        public string? Photo { get; set; } // without http:
        /// <summary>
        /// URL address of products thumbnail image (100x75px)
        /// </summary>
        public string? Thumbnail { get; set; } // without http:
        /// <summary>
        /// Gross weight of 1 pcs of a product
        /// </summary>
        public double Weight { get; set; }
        /// <summary>
        /// Number of products supplied in one package.
        /// </summary>
        public int SuppliedAmount { get; set; }
        /// <summary>
        /// - The minimum amount of product that can be ordered.
        /// </summary>
        public int MinAmount { get; set; }
        /// <summary>
        /// - Product multiplicity. Product Quantity must be a multiple of this value
        /// </summary>
        public int Multiples { get; set; }
        /// <summary>
        /// List of product statuses
        /// </summary>
        public List<string> ProductStatusList { get; set; } = new();
        /// <summary>
        /// Symbol of unit used to describe amount of product e.g. "pcs" (pieces).
        /// </summary>
        public string? Unit { get; set; }
        /// <summary>
        /// ID of available offer
        /// </summary>
        public int? OfferId { get; set; }
        /// <summary>
        /// CustomerSymbol or empty string.
        /// </summary>
        public string? CustomerSymbol { get; set; }
        /// <summary>
        /// Product information URL address.
        /// </summary>
        public string? ProductInformationPage { get; set; } // without http:
        /// <summary>
        /// Information about product's guarantee. Can be null when there is no guarantee for this product
        /// </summary>
        public Guarantee? Guarantee { get; set; }

        // populated by other api calls
        public virtual int QuantityAvailable { get; set; }
        public virtual double Cost { get; set; }
        public virtual string? Currency { get; set; }
        public virtual List<TmeDocument> Datasheets { get; set; } = new();
        public virtual List<TmeDocument> Videos { get; set; } = new();
        public virtual List<TmePhotoFile> Photos { get; set; } = new();
    }

    public class Guarantee
    {
        /// <summary>
        /// Guarantee type - can be "period" or "lifetime"
        /// </summary>
        public string? Type { get; set; } // lifetime

        /// <summary>
        /// Guarantee period in months. This field may be equal "0" in case of lifetime guarantee
        /// </summary>
        public int Period { get; set; }
    }
}
