#!/bin/bash

# Monitor staging deployment and verify queue behavior
STAGING_URL="https://staging-happynotes-api.shukebeta.com"
API_PATH="/api/admin/sync-queue"

echo "🚀 Monitoring Redis Sync Queue deployment to staging..."
echo "📅 Started at: $(date)"
echo

# Function to check API health
check_health() {
    echo "🔍 Checking queue health..."
    curl -s "${STAGING_URL}${API_PATH}/health" | jq '.' 2>/dev/null || echo "❌ Health check failed"
    echo
}

# Function to get queue stats
get_stats() {
    echo "📊 Queue Statistics:"
    echo "==================="
    
    for service in telegram mastodon manticore; do
        echo "📋 $service service:"
        curl -s "${STAGING_URL}${API_PATH}/stats/${service}" | jq '.' 2>/dev/null || echo "❌ Failed to get stats for $service"
        echo
    done
}

# Function to monitor for a period
monitor_period() {
    local duration=${1:-300}  # Default 5 minutes
    local interval=${2:-30}   # Default 30 seconds
    
    echo "⏱️  Monitoring for ${duration} seconds (checking every ${interval}s)..."
    
    end_time=$(($(date +%s) + duration))
    
    while [ $(date +%s) -lt $end_time ]; do
        echo "🔄 Check at $(date +%H:%M:%S):"
        check_health
        get_stats
        echo "---"
        sleep $interval
    done
}

# Main monitoring flow
echo "1️⃣ Initial health check..."
check_health

echo "2️⃣ Initial queue statistics..."
get_stats

echo "3️⃣ Starting continuous monitoring..."
monitor_period 300 60  # Monitor for 5 minutes, check every minute

echo "✅ Monitoring completed at: $(date)"
echo
echo "🔗 Manual verification URLs:"
echo "   Health: ${STAGING_URL}${API_PATH}/health"
echo "   Stats:  ${STAGING_URL}${API_PATH}/stats"
echo "   Telegram: ${STAGING_URL}${API_PATH}/stats/telegram"