-- Sample Lookup Data for LPR System
-- Insert common lookup values for dropdowns and validation

-- Entry Types
INSERT INTO lookup_tables (category, code, name, description, is_active, sort_order, created_at, updated_at) VALUES
('entry_type', 'tenant', 'Tenant', 'Permanent resident of the property', true, 1, NOW(), NOW()),
('entry_type', 'visitor', 'Visitor', 'Temporary visitor to the property', true, 2, NOW(), NOW()),
('entry_type', 'staff', 'Staff', 'Property management or maintenance staff', true, 3, NOW(), NOW()),
('entry_type', 'contractor', 'Contractor', 'External contractor or service provider', true, 4, NOW(), NOW()),
('entry_type', 'delivery', 'Delivery', 'Delivery or courier service', true, 5, NOW(), NOW()),
('entry_type', 'emergency', 'Emergency', 'Emergency services (police, fire, ambulance)', true, 6, NOW(), NOW()),
('entry_type', 'temporary', 'Temporary', 'Temporary access for specific period', true, 7, NOW(), NOW());

-- Recurring Patterns
INSERT INTO lookup_tables (category, code, name, description, is_active, sort_order, created_at, updated_at) VALUES
('recurring_pattern', 'daily', 'Daily', 'Access allowed every day within time window', true, 1, NOW(), NOW()),
('recurring_pattern', 'weekdays', 'Weekdays', 'Access allowed Monday to Friday only', true, 2, NOW(), NOW()),
('recurring_pattern', 'weekends', 'Weekends', 'Access allowed Saturday and Sunday only', true, 3, NOW(), NOW()),
('recurring_pattern', 'weekly', 'Weekly', 'Access allowed on specific day of week', true, 4, NOW(), NOW()),
('recurring_pattern', 'monthly', 'Monthly', 'Access allowed on specific day of month', true, 5, NOW(), NOW()),
('recurring_pattern', 'custom', 'Custom', 'Custom recurring pattern', true, 6, NOW(), NOW());

-- Entry Status
INSERT INTO lookup_tables (category, code, name, description, is_active, sort_order, created_at, updated_at) VALUES
('entry_status', 'allowed', 'Allowed', 'Entry was permitted and gate opened', true, 1, NOW(), NOW()),
('entry_status', 'denied', 'Denied', 'Entry was denied - plate not in whitelist', true, 2, NOW(), NOW()),
('entry_status', 'expired', 'Expired', 'Entry denied - whitelist entry has expired', true, 3, NOW(), NOW()),
('entry_status', 'exceeded_limit', 'Exceeded Limit', 'Entry denied - maximum entries reached', true, 4, NOW(), NOW()),
('entry_status', 'blacklisted', 'Blacklisted', 'Entry denied - plate is blacklisted', true, 5, NOW(), NOW()),
('entry_status', 'time_restricted', 'Time Restricted', 'Entry denied - outside allowed time window', true, 6, NOW(), NOW());

-- Vehicle Color Types (from LPR documentation)
INSERT INTO lookup_tables (category, code, numeric_value, name, description, is_active, sort_order, created_at, updated_at) VALUES
('color_type', 'white', 0, 'White', 'White colored vehicle', true, 1, NOW(), NOW()),
('color_type', 'black', 1, 'Black', 'Black colored vehicle', true, 2, NOW(), NOW()),
('color_type', 'red', 2, 'Red', 'Red colored vehicle', true, 3, NOW(), NOW()),
('color_type', 'blue', 3, 'Blue', 'Blue colored vehicle', true, 4, NOW(), NOW()),
('color_type', 'yellow', 4, 'Yellow', 'Yellow colored vehicle', true, 5, NOW(), NOW()),
('color_type', 'green', 5, 'Green', 'Green colored vehicle', true, 6, NOW(), NOW()),
('color_type', 'gray', 6, 'Gray', 'Gray colored vehicle', true, 7, NOW(), NOW()),
('color_type', 'silver', 7, 'Silver', 'Silver colored vehicle', true, 8, NOW(), NOW()),
('color_type', 'brown', 8, 'Brown', 'Brown colored vehicle', true, 9, NOW(), NOW()),
('color_type', 'other', 9, 'Other', 'Other or unknown color', true, 10, NOW(), NOW());

-- Vehicle Directions
INSERT INTO lookup_tables (category, code, numeric_value, name, description, is_active, sort_order, created_at, updated_at) VALUES
('direction', 'unknown', 0, 'Unknown', 'Direction not determined', true, 1, NOW(), NOW()),
('direction', 'left_to_right', 1, 'Left to Right', 'Vehicle moving from left to right', true, 2, NOW(), NOW()),
('direction', 'right_to_left', 2, 'Right to Left', 'Vehicle moving from right to left', true, 3, NOW(), NOW()),
('direction', 'top_to_bottom', 3, 'Top to Bottom', 'Vehicle moving from top to bottom', true, 4, NOW(), NOW()),
('direction', 'bottom_to_top', 4, 'Bottom to Top', 'Vehicle moving from bottom to top', true, 5, NOW(), NOW());

-- Trigger Types
INSERT INTO lookup_tables (category, code, numeric_value, name, description, is_active, sort_order, created_at, updated_at) VALUES
('trigger_type', 'auto', 0, 'Auto', 'Automatic trigger by motion detection', true, 1, NOW(), NOW()),
('trigger_type', 'manual', 1, 'Manual', 'Manual trigger by operator', true, 2, NOW(), NOW()),
('trigger_type', 'external', 2, 'External', 'External trigger signal', true, 3, NOW(), NOW()),
('trigger_type', 'scheduled', 3, 'Scheduled', 'Scheduled automatic trigger', true, 4, NOW(), NOW());

-- Vehicle Types
INSERT INTO lookup_tables (category, code, numeric_value, name, description, is_active, sort_order, created_at, updated_at) VALUES
('vehicle_type', 'unknown', 0, 'Unknown', 'Vehicle type not determined', true, 1, NOW(), NOW()),
('vehicle_type', 'car', 1, 'Car', 'Passenger car', true, 2, NOW(), NOW()),
('vehicle_type', 'suv', 2, 'SUV', 'Sport Utility Vehicle', true, 3, NOW(), NOW()),
('vehicle_type', 'truck', 3, 'Truck', 'Truck or lorry', true, 4, NOW(), NOW()),
('vehicle_type', 'van', 4, 'Van', 'Van or minivan', true, 5, NOW(), NOW()),
('vehicle_type', 'motorcycle', 5, 'Motorcycle', 'Motorcycle or scooter', true, 6, NOW(), NOW()),
('vehicle_type', 'bus', 6, 'Bus', 'Bus or coach', true, 7, NOW(), NOW()),
('vehicle_type', 'trailer', 7, 'Trailer', 'Trailer or semi-trailer', true, 8, NOW(), NOW());

-- Device Status
INSERT INTO lookup_tables (category, code, name, description, is_active, sort_order, created_at, updated_at) VALUES
('device_status', 'online', 'Online', 'Device is connected and responding', true, 1, NOW(), NOW()),
('device_status', 'offline', 'Offline', 'Device is not responding', true, 2, NOW(), NOW()),
('device_status', 'maintenance', 'Maintenance', 'Device is under maintenance', true, 3, NOW(), NOW()),
('device_status', 'error', 'Error', 'Device has reported an error', true, 4, NOW(), NOW());

-- Command Types
INSERT INTO lookup_tables (category, code, name, description, is_active, sort_order, created_at, updated_at) VALUES
('command_type', 'open_gate', 'Open Gate', 'Command to open the gate', true, 1, NOW(), NOW()),
('command_type', 'close_gate', 'Close Gate', 'Command to close the gate', true, 2, NOW(), NOW()),
('command_type', 'reboot', 'Reboot', 'Command to reboot the device', true, 3, NOW(), NOW()),
('command_type', 'update_firmware', 'Update Firmware', 'Command to update device firmware', true, 4, NOW(), NOW()),
('command_type', 'whitelist_add', 'Add Whitelist', 'Command to add whitelist entry', true, 5, NOW(), NOW()),
('command_type', 'whitelist_remove', 'Remove Whitelist', 'Command to remove whitelist entry', true, 6, NOW(), NOW()),
('command_type', 'whitelist_clear', 'Clear Whitelist', 'Command to clear all whitelist entries', true, 7, NOW(), NOW());
