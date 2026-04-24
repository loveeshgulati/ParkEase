using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkEase.Payment.Migrations
{
    /// <inheritdoc />
    public partial class AddRazorpayColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Add razorpay_order_id column if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'payments' AND column_name = 'razorpay_order_id') THEN
                        ALTER TABLE payments ADD COLUMN razorpay_order_id text;
                    END IF;
                    
                    -- Add razorpay_payment_id column if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'payments' AND column_name = 'razorpay_payment_id') THEN
                        ALTER TABLE payments ADD COLUMN razorpay_payment_id text;
                    END IF;
                    
                    -- Create indexes for the new columns if they don't exist
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE tablename = 'payments' AND indexname = 'IX_payments_razorpay_order_id') THEN
                        CREATE INDEX IX_payments_razorpay_order_id ON payments(razorpay_order_id);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE tablename = 'payments' AND indexname = 'IX_payments_razorpay_payment_id') THEN
                        CREATE INDEX IX_payments_razorpay_payment_id ON payments(razorpay_payment_id);
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
