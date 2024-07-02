if ARGV.count != 1
  puts "Enter a single directory containing .rb files"
  return
end

dir = ARGV[0]

files = Dir.entries(dir).filter { |fn| fn.end_with?('.rb') }.sort
puts "Found #{files.count} files in \"#{dir}\"."

Dir.chdir(dir) do
  files.each do |file|
    lines = []

    # Gather only the method names in method definitions.
    IO.foreach(file) do |line|
      break if line.match? "^\s+private"

      method_name = /^\s+def (.+?)(?:; end|\(.*\))?$/.match line
      lines << method_name.captures[0] if method_name
    end

    puts "#{file}"
    lines.each { |l| puts "  - #{l}" }
  end
end
