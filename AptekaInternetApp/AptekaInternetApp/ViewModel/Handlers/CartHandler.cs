using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace AptekaInternetApp.ViewModel
{
    static public class CartHandler
    {
        private static readonly string CartFilePath = Path.Combine(
       Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
       "Resources", "ShoppingCart.json");

        // Событие для уведомления об изменении корзины (productId, newQuantity)
        public static event Action<int, int> CartChanged;

        public static void HandleCartProductSelection(int productId, int quantity = 1)
        {
            AddToCart(productId, quantity);
        }

        public static decimal GetTotalAmountWithDiscount()
        {
            var cart = LoadCart();
            return cart.Sum(item => item.PriceWithDiscount * item.Quantity);
        }

        public static decimal GetProductDiscount(int productId)
        {
            using (var context = new ContextDB())
            {
                var now = DateTime.Now;
                var activeDiscount = context.Discounts
                    .FirstOrDefault(d => d.ProductId == productId &&
                                       d.IsActive &&
                                       d.StartDate <= now &&
                                       d.EndDate >= now);

                return activeDiscount?.Percent ?? 0;
            }
        }

        public static decimal GetTotalDiscount()
        {
            var cart = LoadCart();
            decimal totalWithoutDiscount = cart.Sum(item => item.Price * item.Quantity);
            decimal totalWithDiscount = cart.Sum(item => item.PriceWithDiscount * item.Quantity);
            return totalWithoutDiscount - totalWithDiscount;
        }

        public static void AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var cart = LoadCart();
                var existingItem = cart.FirstOrDefault(p => p.Id == productId);

                int newQuantity = 0;

                if (existingItem != null)
                {
                    using (var context = new ContextDB())
                    {
                        var stock = context.Stocks.FirstOrDefault(s => s.ProductId == productId);
                        if (stock != null && existingItem.Quantity + quantity <= stock.Quantity)
                        {
                            existingItem.Quantity += quantity;
                            newQuantity = existingItem.Quantity;
                        }
                        else
                        {
                            MessageBox.Show("Недостаточно товара на складе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            
                            return;
                        }
                    }
                }
                else
                {
                    using (var context = new ContextDB())
                    {
                        var product = context.Products.FirstOrDefault(p => p.Id == productId);
                        var stock = context.Stocks.FirstOrDefault(s => s.ProductId == productId);

                        if (product != null && stock != null && quantity <= stock.Quantity)
                        {
                            // РАССЧИТЫВАЕМ СКИДКУ ТОЛЬКО ЗДЕСЬ
                            var discountPercent = GetProductDiscount(product.Id);

                            var cartProduct = new CartProduct
                            {
                                Id = product.Id,
                                NameProduct = $"{product.Name} {product.ReleaseForm} {product.PackageContentsQuantity}",
                                Price = product.Price,
                                Quantity = quantity,
                                ImageCatalogProduct = product.ImageUrl,
                                StockQuantity = stock.Quantity,
                                RequiresPrescription = product.RequiresPrescription,
                                DiscountPercent = discountPercent,
                              
                            };
                            cart.Add(cartProduct);
                            newQuantity = quantity;
                        }
                        else
                        {
                            MessageBox.Show("Недостаточно товара на складе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                SaveCart(cart);
                OnCartChanged(productId, newQuantity);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в корзину: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void UpdateQuantity(int productId, int newQuantity)
        {
            try
            {
                var cart = LoadCart();
                var item = cart.FirstOrDefault(p => p.Id == productId);

                if (item != null)
                {
                    if (newQuantity <= 0)
                    {
                        cart.Remove(item);
                        newQuantity = 0;
                    }
                    else if (newQuantity <= item.StockQuantity)
                    {
                        item.Quantity = newQuantity;
                    }
                    else
                    {
                        MessageBox.Show("Недостаточно товара на складе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    SaveCart(cart);
                    OnCartChanged(productId, newQuantity);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении количества: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void RemoveFromCart(int productId)
        {
            try
            {
                var cart = LoadCart();
                var item = cart.FirstOrDefault(p => p.Id == productId);

                if (item != null)
                {
                    cart.Remove(item);
                    SaveCart(cart);
                    OnCartChanged(productId, 0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении из корзины: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static List<CartProduct> LoadCart()
        {
            try
            {
                if (File.Exists(CartFilePath))
                {
                    string json = File.ReadAllText(CartFilePath);
                    return JsonConvert.DeserializeObject<List<CartProduct>>(json) ?? new List<CartProduct>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new List<CartProduct>();
        }

        public static void SaveCart(List<CartProduct> cart)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CartFilePath));
                string json = JsonConvert.SerializeObject(cart, Formatting.Indented);
                File.WriteAllText(CartFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения корзины: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static int GetTotalItems()
        {
            var cart = LoadCart();
            return cart.Sum(item => item.Quantity);
        }

        public static decimal GetTotalAmount()
        {
            var cart = LoadCart();
            return cart.Sum(item => item.Price * item.Quantity);
        }

        public static int GetProductQuantity(int productId)
        {
            var cart = LoadCart();
            var item = cart.FirstOrDefault(p => p.Id == productId);
            return item?.Quantity ?? 0;
        }

        public static bool IsProductInCart(int productId)
        {
            var cart = LoadCart();
            return cart.Any(p => p.Id == productId);
        }

        public static void ClearCart()
        {
            try
            {
                SaveCart(new List<CartProduct>());
                OnCartChanged(0, 0); // Уведомляем об очистке корзины
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при очистке корзины: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void OnCartChanged(int productId, int newQuantity)
        {
            CartChanged?.Invoke(productId, newQuantity);
        }
    }
}
