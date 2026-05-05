using System;

namespace Hardware_Shop
{
    // ============================================================
    //  ENUMERATION — OOP Requirement
    //  بدل ما الـ Category تتخزن كـ string حر (ممكن يحصل فيه typo)،
    //  بنعرّف enum ثابتة بكل الأصناف الممكنة.
    // ============================================================

    /// <summary>
    /// Represents the valid product categories available in the hardware shop.
    /// Using an enum prevents invalid category values and enables compile-time checking.
    /// </summary>
    public enum ProductCategory
    {
        Tools,
        Electrical,
        Plumbing,
        Fasteners,
        Safety,
        Paint,
        Other
    }
}
