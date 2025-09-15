-- Migration: Add Screenshot Feature
-- Date: 2025-09-15

-- 1. Add screenshot capture fields to devices table
ALTER TABLE devices ADD COLUMN IF NOT EXISTS capture_screenshot_enabled BOOLEAN DEFAULT false;
ALTER TABLE devices ADD COLUMN IF NOT EXISTS screenshot_capture_status VARCHAR(20);
ALTER TABLE devices ADD COLUMN IF NOT EXISTS last_screenshot_request TIMESTAMP WITH TIME ZONE;

-- 2. Create plate_recognition_screenshots table
CREATE TABLE IF NOT EXISTS plate_recognition_screenshots (
    id SERIAL PRIMARY KEY,
    
    -- Links
    plate_recognition_id INTEGER NOT NULL REFERENCES plate_recognition_results(id) ON DELETE CASCADE,
    site_id INTEGER NOT NULL REFERENCES sites(id),
    device_id INTEGER NOT NULL REFERENCES devices(id),
    
    -- Image data
    image_base64 TEXT NOT NULL,
    image_length INTEGER NOT NULL,
    image_format VARCHAR(10) NOT NULL DEFAULT 'jpeg',
    
    -- Recognition context
    license_plate VARCHAR(20) NOT NULL,
    recognition_result VARCHAR(20) NOT NULL, -- 'allowed', 'denied', 'unknown', 'expired'
    confidence_score DECIMAL(5,2),
    
    -- Processing status
    screenshot_status VARCHAR(20) NOT NULL DEFAULT 'pending', -- 'pending', 'received', 'failed', 'timeout'
    requested_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    received_at TIMESTAMP WITH TIME ZONE,
    
    -- Metadata
    camera_ip VARCHAR(45),
    trigger_source VARCHAR(30) NOT NULL DEFAULT 'plate_recognition',
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- 3. Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_plate_screenshots_plate_recognition ON plate_recognition_screenshots(plate_recognition_id);
CREATE INDEX IF NOT EXISTS idx_plate_screenshots_device ON plate_recognition_screenshots(device_id);
CREATE INDEX IF NOT EXISTS idx_plate_screenshots_license_plate ON plate_recognition_screenshots(license_plate);
CREATE INDEX IF NOT EXISTS idx_plate_screenshots_result ON plate_recognition_screenshots(recognition_result);
CREATE INDEX IF NOT EXISTS idx_plate_screenshots_status ON plate_recognition_screenshots(screenshot_status);
CREATE INDEX IF NOT EXISTS idx_plate_screenshots_created ON plate_recognition_screenshots(created_at);

-- 4. Add screenshot tracking to plate_recognition_results table
ALTER TABLE plate_recognition_results ADD COLUMN IF NOT EXISTS screenshot_requested BOOLEAN DEFAULT false;
ALTER TABLE plate_recognition_results ADD COLUMN IF NOT EXISTS screenshot_status VARCHAR(20);

-- 5. Update existing devices to have screenshot capture disabled by default
UPDATE devices SET capture_screenshot_enabled = false WHERE capture_screenshot_enabled IS NULL;

-- 6. Create view for easy screenshot querying
CREATE OR REPLACE VIEW screenshot_summary AS
SELECT 
    prs.id,
    prs.license_plate,
    prs.recognition_result,
    prs.screenshot_status,
    prs.requested_at,
    prs.received_at,
    prs.image_length,
    prs.image_format,
    pr.recognition_timestamp,
    d.device_name,
    d.serial_number,
    s.site_code,
    s.site_name,
    CASE 
        WHEN prs.received_at IS NOT NULL THEN 
            EXTRACT(EPOCH FROM (prs.received_at - prs.requested_at))
        ELSE NULL 
    END as response_time_seconds
FROM plate_recognition_screenshots prs
JOIN plate_recognition_results pr ON prs.plate_recognition_id = pr.id
JOIN devices d ON prs.device_id = d.id
JOIN sites s ON prs.site_id = s.id;
