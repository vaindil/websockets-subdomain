CREATE TABLE key_value (
  "key" text NOT NULL,
  value text NULL,
  CONSTRAINT key_value_pkey PRIMARY KEY (key)
);

CREATE TABLE twitch_webhook_notification (
  id text NOT NULL PRIMARY KEY,
  received_at timestamptz NOT NULL,
  user_id text NOT NULL,
  username text NOT NULL,
  game_id text NULL,
  title text NULL,
  started_at timestamptz NULL
);

-- https://www.the-art-of-web.com/sql/trigger-delete-old/
CREATE FUNCTION delete_old_notifications() RETURNS TRIGGER
  LANGUAGE plpgsql
  AS $$
BEGIN
  DELETE FROM twitch_webhook_notification WHERE received_at < NOW() - INTERVAL '3 days';
  RETURN NULL;
END;
$$;

CREATE TRIGGER trigger_delete_old_notifications
  AFTER INSERT ON twitch_webhook_notification
  EXECUTE PROCEDURE delete_old_notifications();